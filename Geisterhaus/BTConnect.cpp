// BTConnect.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "BTConnect.h"
#include "stdlib.h"

#pragma comment ( lib, "Bthprops.lib" )
#pragma comment ( lib, "Ws2_32.lib" )
#pragma warning ( disable : 4995 )
#pragma warning ( disable : 4996 )

// Global resources
WCHAR		g_pServerGUID[] = L"2E8025D9-EF25-48b3-9DAA-8F589F82223C";
GUID		g_serverGUID;

// 128Bit-Key
char		g_Key [ 17 ];
char		g_KeyCR [ 17 ];


bool new_msg_received = false;
char msg_id = 'x';

// Pre-Shared-Key for exchanging the 128Bit-Key
char PSK [] = "UniAugIntSurfGrp";


struct __DeviceList {
	int				Count;
	BTDevice	*	Start;
	BTDevice	*	Last;
} g_DeviceList;

bool		g_Initialized	= false;
SOCKET		MsgInSocket		= INVALID_SOCKET;


// Forward declaratios




/*
 * FUNCTION: NaiveVigenere ( )
 *
 * PURPOSE:  A naive vigenere encrypter for exchanging the PSK
 *
 */
void NaiveVigenere ( int size, char * buffer )
{
	int len = strlen(PSK);
	for ( int i = 0; i<len; i++ )
		PSK[i] -= 0x41;

	int i = 0;
	while ( size > 0 )
	{
		if ( i > len )
			i = 0;

		*buffer += PSK[i];
		buffer++;
		i++;
		size--;
	}
}

void NaiveVigenereDec ( int size, char * buffer )
{
	int len = strlen(PSK);
	for ( int i = 0; i<len; i++ )
		PSK[i] -= 0x41;

	int i = 0;
	while ( size > 0 )
	{
		if ( i > len )
			i = 0;

		*buffer -= PSK[i];
		buffer++;
		i++;
		size--;
	}
}


/*
 * FUNCTION: EnDeCrypt ( )
 *
 * PURPOSE:  A simple XOR-EnDeCrypt
 *
 */
void EnDeCrypt ( bool Decrypt, int size, char buffer[] )
{
	ULONGLONG			Key;
	ULONGLONG			KeyLow		= *( (ULONGLONG *) g_Key );
	ULONGLONG			KeyHigh		= *( (ULONGLONG *) (g_Key + 8) );

	// Get KeyOffset from the first byte
	unsigned char	*	pOffset		= (unsigned char *) buffer;
	unsigned char		offset		= *pOffset;
	unsigned char	*	z;
	unsigned char		offsetXord	= offset ^ (*( (unsigned char *) &KeyLow ));

	if ( Decrypt )
	{
		// Use UnCrypted Offset with unrolled key
		offset = offsetXord;
	}

	int			s			= sizeof(ULONGLONG);

	// Roll the Key
	if ( offset > 0 )
	{
		int bits = s * 8;
		if ( offset < bits )
		{
			ULONGLONG newLow = KeyLow << offset;
			newLow |= (KeyHigh >> (bits-offset));
			
			ULONGLONG newHigh = KeyHigh << offset;
			newHigh |= (KeyLow >> (bits-offset));

			KeyLow = newLow;
			KeyHigh = newHigh;
		}
		else if ( offset < bits*2 )
		{
			offset -= bits;
			ULONGLONG newLow = KeyHigh << offset;
			newLow |= (KeyLow >> (bits-offset));
			
			ULONGLONG newHigh = KeyLow << offset;
			newHigh |= (KeyHigh >> (bits-offset));

			KeyLow = newLow;
			KeyHigh = newHigh;
			
			offset += bits;
		}
		else
			return;
	}

	bool		c			= false;

	while ( size > 0 )
	{
		if ( c )
			Key = KeyLow;
		else
			Key = KeyHigh;

		if ( size >= s )
		{
			ULONGLONG * p = (ULONGLONG *) buffer;
			*p ^= Key;
			buffer += s;
			size -= s;
		}
		else
		{
			z = (unsigned char *) &Key;
			while ( size )
			{
				*buffer ^= *z;
				size--;
				z++;
				buffer++;
			}
		}
	}

	if ( !Decrypt )
	{
		// Encrypt Offset with unrolled key
		*pOffset = offsetXord;
	}
}

int CreateClientSocket ( BTDevice * dev )
{
	if ( dev->Socket == INVALID_SOCKET )
	{
		SOCKET socketClient = socket ( AF_BTH, SOCK_STREAM, BTHPROTO_RFCOMM );

		if ( socketClient == INVALID_SOCKET )
			return WSAGetLastError();
			
		SOCKADDR_BTH SocketAddress;
		SocketAddress.btAddr			= dev->Address;
		SocketAddress.port				= 0;
		SocketAddress.addressFamily		= AF_BTH;	
		SocketAddress.serviceClassId	= g_serverGUID;

		if ( connect ( socketClient, (SOCKADDR *)&SocketAddress, sizeof(SocketAddress) ) == SOCKET_ERROR ) 
		{
			socketClient = INVALID_SOCKET;
			return WSAGetLastError();
		}

		dev->Socket = socketClient;
		dev->SocketAddress = SocketAddress;
	}
	return 0;
}

/*
 * FUNCTION: SendMessage ( )
 *
 * PURPOSE:  Send Buffer to Client
 *
 */
int SendMessage ( BTDevice * dev, int length, char * msg, bool encode = true )
{
	if ( dev->Socket == INVALID_SOCKET )
	{
		int ret = CreateClientSocket ( dev );
		if ( ret != 0 )
			return ret;
	}

	dev->ACK = false;
	ResetEvent ( dev->hReceiveEvent );

    if ( length > 1 ) 
    {
		if ( encode )
		{
			*msg = (char) (rand() % 128);

			EnDeCrypt ( false, length, msg );
		}

		int bytesSent = send ( dev->Socket, (char *)msg, length, 0 );
		if ( bytesSent != length )
        {
	        return WSAGetLastError ();
        }
	}
	return 0;
}



/*
 * FUNCTION: WaitForACK ( )
 *
 * PURPOSE:
 *
 * RETURN:
 *
 */
BOOL WaitForACK ( BTDevice * dev )
{
	if ( WaitForSingleObject ( dev->hReceiveEvent, INFINITE ) != WAIT_OBJECT_0 )
		return false;

	return dev->ACK;
}


/*
 * FUNCTION: WaitForByte ( )
 *
 * PURPOSE:
 *
 * RETURN:
 *
 */
unsigned char WaitForByte ( BTDevice * dev )
{
	if ( WaitForSingleObject ( dev->hReceiveEvent, INFINITE ) != WAIT_OBJECT_0 )
		return false;

	return dev->byte;
}


/*
 * FUNCTION: SearchThread ( )
 *
 * PURPOSE:  A Thread for listening to the service-socket (to the client) and handle the requests
 *
 */
DWORD WINAPI SearchThread ( LPVOID arg ) 
{
	union {
		CHAR buffer [ MAX_BUFF_SIZE ];
		double __unused;
	};
	
	while ( true )
	{
		WSAQUERYSET		qset;
		memset ( &qset, 0, sizeof(qset) );
		qset.dwSize			= sizeof(qset);
		qset.dwNameSpace	= NS_BTH;

		HANDLE	hSearch = NULL;
		DWORD	dwFlags = LUP_CONTAINERS | LUP_FLUSHCACHE;
		if ( WSALookupServiceBegin ( &qset, dwFlags, &hSearch ) )
		{
			long lerr = WSAGetLastError();
			return -1;
		}
	}
	return 0;
}


/*
 * FUNCTION: ListenThread ( )
 *
 * PURPOSE:  A Thread for listening to the service-socket (to the client) and handle the requests
 *
 */
DWORD WINAPI ListenThread ( LPVOID arg ) 
{
	char	Buffer		[ MAX_MESSAGE_SIZE ];
	
	// A reference to the bt-object
	BTDevice * dev = (BTDevice *) arg;

	int iSize = sizeof(dev->Socket);
	
	while ( true ) 
	{
		// Receive data
		int BytesReceived = recv ( dev->Socket, Buffer, MAX_MESSAGE_SIZE, 0 );

		// If error occured in receiving, then quit connection and go to next connection
		if ( BytesReceived == SOCKET_ERROR ) 
		{
			int lerr = WSAGetLastError();
			break;
		}

		if ( !BytesReceived )
		{
			//(*dev->AddToMessages) ( L"Client has disconnected." );
			break;
		}

		EnDeCrypt ( true, BytesReceived, Buffer );

		// Check for AKN
		if ( BytesReceived == 2 )
		{
			if ( Buffer [ 1 ] == MSG_ACK )
				dev->ACK = TRUE;
			else if ( Buffer [1] == MSG_SIGNAL_C )
			{
new_msg_received = true;
msg_id = 'C';
			}
else if ( Buffer [1] == MSG_SIGNAL_D )
			{
new_msg_received = true;
msg_id = 'D';
			}

			
		}
		
		else if ( BytesReceived == 3 )
		{
			if ( Buffer [ 1 ] == MSG_DATA_BYTE )
				dev->byte = Buffer [ 2 ];
		}

		SetEvent ( dev->hReceiveEvent );
	}

	// Signal leaving the thread
	SetEvent ( dev->hReceiveEvent );

	// Delete Socket
	if ( dev->Socket )
	{
		closesocket ( dev->Socket );
		dev->Socket = INVALID_SOCKET;
	}

	// Delete Event
	if ( dev->hReceiveEvent )
	{
		CloseHandle ( dev->hReceiveEvent );
		dev->hReceiveEvent = NULL;
	}

	dev->hListenThread = NULL;
	return 0;
}

EXPORT
char GetReceivedMsg()
{
	if (new_msg_received)
	{
		new_msg_received = false;
		return msg_id;
	}
	else return 'x';
}



/*
 * FUNCTION: Init ( )
 *
 * PURPOSE:  Initialize global resources used within this dll
 *
 */
EXPORT
bool Init ( )
{
	// Initialize 128Bit Key
	srand ( GetTickCount() );

	int		i;
	char	c;

	for ( i=0; i<16; i++ )
	{
		c = rand() % 255;
		g_Key[i] = c;
		g_KeyCR[i] = c;
	}
	g_KeyCR[16] = 0;

	// Encode Key with vigenere and our pre-shared-key
	NaiveVigenere ( 16, g_KeyCR );

	/*
	char msg[] = " This is a test message that has to be encrypted!!!";
	*msg = 68;

	int len = strlen(msg);
	EnDeCrypt ( false, len, msg );
	EnDeCrypt ( true, len, msg );


	NaiveVigenereDec ( strlen(g_KeyCR), g_KeyCR );
	*/

	if ( g_Initialized )
		return TRUE;

	// Init DeviceList
	memset ( &g_DeviceList, 0, sizeof(g_DeviceList) );

	// Init GUID
	int data1, data2, data3;
	int data4[8];

	if ( 11 == swscanf ( g_pServerGUID, L"%08x-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x\n",
					&data1, &data2, &data3,
					&data4[0], &data4[1], &data4[2], &data4[3], 
					&data4[4], &data4[5], &data4[6], &data4[7])) {
		g_serverGUID.Data1 = data1;
		g_serverGUID.Data2 = data2 & 0xffff;
		g_serverGUID.Data3 = data3 & 0xffff;

		for (int i = 0 ; i < 8 ; ++i)
			g_serverGUID.Data4[i] = data4[i] & 0xff;
	}
	else
		return FALSE;

	// Init WinSock	
	WSADATA			wsaData;
	memset ( &wsaData, 0, sizeof(wsaData) );

	if ( WSAStartup ( MAKEWORD ( 2, 2 ), &wsaData ) != ERROR_SUCCESS )
		return FALSE;

	g_Initialized = TRUE;

	/*
	HANDLE hThread = CreateThread ( NULL, 0, SearchThread, NULL, 0, NULL );
	CloseHandle ( hThread );
	*/
	
	return TRUE;
}


/*
 * FUNCTION: Release ( )
 *
 * PURPOSE:  Release resources
 *
 */
EXPORT
void Release ( )
{
	if ( g_Initialized )
	{
		WSACleanup ();
		g_Initialized = FALSE;
	}

	// Release DeviceList
	BTDevice * dev = g_DeviceList.Start;
	while ( dev )
	{
		BTDevice * del = dev;
		dev = del->Next;

		if ( del->Socket != INVALID_SOCKET )
		{
			closesocket ( del->Socket );
			del->Socket = INVALID_SOCKET;
		}

		Sleep ( 10 );
		if ( del->hListenThread )
		{
			TerminateThread ( del->hListenThread, -1 );
			del->hListenThread = NULL;
		}

		if ( del->hReceiveEvent )
		{
			CloseHandle ( del->hReceiveEvent );
			del->hReceiveEvent = NULL;
		}
		delete del;
	}
}


/*
 * FUNCTION: LookUpService ( )
 *
 * PURPOSE:  
 *
 */
EXPORT
bool LookUpService ( LPCSADDR_INFO addrInfo )
{
	bool			supportProtocol = false;
	WSAQUERYSET		qset;
	HANDLE			hSearch;

	LPWSAQUERYSET	pResults;
	union {
		CHAR buffer [ MAX_BUFF_SIZE ];
		double __unused;	// ensure proper alignment? taken from sample
	};
	
	DWORD			dwSize;
	LPWSTR			pName;

	DWORD			dwFlags;
	WCHAR			szAddress [ 1024 ];
	dwSize	= 1024;

	hSearch = NULL;

    memset ( &qset, 0, sizeof(qset) );
	qset.dwSize = sizeof(qset);
	qset.dwNameSpace = NS_BTH;
	//GUID protocol = ServiceDiscoveryServerServiceClassID_UUID;
	//GUID protocol = L2CAP_PROTOCOL_UUID;

	qset.lpServiceClassId = &g_serverGUID;

	if ( WSAAddressToString ( addrInfo->RemoteAddr.lpSockaddr, addrInfo->RemoteAddr.iSockaddrLength, NULL, szAddress, &dwSize ) )
	{
		printf ( "\t-- Invalid BlueTooth Address --\n" );
		goto End;
	}

	qset.lpszContext = szAddress;

	dwFlags = LUP_CONTAINERS | LUP_FLUSHCACHE;
	dwFlags = LUP_FLUSHCACHE | LUP_RETURN_NAME | LUP_RETURN_TYPE | LUP_RETURN_ADDR | LUP_RETURN_BLOB | LUP_RETURN_COMMENT;
	if ( WSALookupServiceBegin ( &qset, dwFlags, &hSearch ) )
	{
		printf ( "\t-- No Service Found --\n" );
		goto End;
	}

	pResults = (LPWSAQUERYSET) buffer;
	memset ( pResults, 0, MAX_BUFF_SIZE );
	pResults->dwSize		= sizeof(WSAQUERYSET);
	pResults->dwNameSpace	= NS_BTH;
	
	dwFlags = LUP_RETURN_NAME | LUP_RETURN_ADDR | LUP_RETURN_ALL;
	dwFlags = LUP_FLUSHCACHE | LUP_RETURN_NAME | LUP_RETURN_TYPE | LUP_RETURN_ADDR | LUP_RETURN_BLOB | LUP_RETURN_COMMENT;

	while ( true )
	{
		dwSize  = MAX_BUFF_SIZE;
		if ( WSALookupServiceNext ( hSearch, dwFlags, &dwSize, pResults ) )
			break;

		pName = pResults->lpszServiceInstanceName;
		if ( pName && *pName )
			wprintf ( L"\tService: %s\n", pName );

		pName = pResults->lpszComment;
		if ( pName && *pName )
			wprintf ( L"\tComment: %s\n", pName );

		wprintf ( L"\n", pName );

		supportProtocol = true;
	}
    
	WSALookupServiceEnd ( hSearch );

End:
	int r = WSAGetLastError ();
	return supportProtocol;
}

void RemoveDeadDevices ( )
{
	// Remove dead entries
	BTDevice * prev = g_DeviceList.Start;
	if ( !prev )
	{
		memset ( &g_DeviceList, 0, sizeof(g_DeviceList) );
		return;
	}

	BTDevice * dev = prev->Next;
	if ( !dev )
	{
		if ( !prev->Alive )
		{
			delete prev;
			memset ( &g_DeviceList, 0, sizeof(g_DeviceList) );
		}
		return;
	}

	while ( dev )
	{
		if ( !dev->Alive )
		{
			if ( dev->hasSupport )
				g_DeviceList.Count--;

			// Terminate Thread if its living

			// Renode the list
			prev->Next = dev->Next;

			// Delete dead entry
			delete dev;

			// handle next
			dev = prev->Next;
		}
		else
		{
			prev = dev;
			dev = dev->Next;
		}
	}

	// Update Last Entry
	dev = g_DeviceList.Start;
	if ( !dev )
	{
		memset ( &g_DeviceList, 0, sizeof(g_DeviceList) );
		return;
	}

	while ( dev->Next )
		dev = dev->Next;
	g_DeviceList.Last = dev;
}


BTDevice * GetDevice ( int devNum )
{
	// Send bit to device
	BTDevice * dev = g_DeviceList.Start;
	while ( dev )
	{
		if ( dev->hasSupport )
		{
			if ( devNum == 0 )
				return dev;
			devNum--;
		}

		dev = dev->Next;
	}
	return NULL;
}



/*
 * FUNCTION: SendBitToDevice ( )
 *
 * PURPOSE:
 *
 * RETURN:
 *
 */
EXPORT

// ATTENTION HERE !!!
int SendCaptureBitToDevice ( int devNum, int command = 0 )
{
	// Send bit to device
	BTDevice * dev = GetDevice ( devNum );
	if ( !dev )
		return 0;
	
	// SendMessage to Device
	unsigned char cap = MSG_CAPTURE_BIT;
	if ( command == -1 )
		cap = MSG_GAMEOVER;
	else if ( command == -2 )
		cap = MSG_GAMESTART;
	else if (command == -3)
		cap = MSG_KATJA;
	else if (command == -4)
		cap = MSG_GESTRIGHT;
	else if (command == -5)
		cap = MSG_GESTWRONG;

	char msg [] = { 0x01, cap, 0x00 };

	if ( SendMessage ( dev, strlen(msg), msg ) )
		return 0;

	// Wait for ACK
	if ( !WaitForACK ( dev ) )
		return 0;
	return 1;
}


/*
 * FUNCTION: GetIdentifierFromDevice ( )
 *
 * PURPOSE:
 *
 * RETURN:
 *
 */
EXPORT
int GetIdentifierFromDevice ( int devNum )
{
	// Send request to device
	BTDevice * dev = GetDevice ( devNum );
	if ( !dev )
		return 0;
	
	char msg [] = { 0x01, MSG_GET_NEGOTIATED_ID, 0x00 };

	if ( SendMessage ( dev, strlen(msg), msg ) )
		return 0;

	return WaitForByte ( dev );
}


/*
 * FUNCTION: SendHelloToDevice ( )
 *
 * PURPOSE:
 *
 * RETURN:
 *
 */
EXPORT
int SendHelloToDevice ( int devNum )
{
	// Get device object
	BTDevice * dev = GetDevice ( devNum );
	if ( !dev )
		return 0;

	if ( dev->Socket == INVALID_SOCKET )
	{
		int ret = CreateClientSocket ( dev );
		if ( ret != 0 )
			return 0;
	}

	// Create Listen Thread
	if ( !dev->hReceiveEvent )
	{
		dev->hReceiveEvent = CreateEvent ( NULL, TRUE, FALSE, NULL );
	}

	// Create Listen Thread
	if ( !dev->hListenThread )
	{
		dev->hListenThread = CreateThread ( NULL, 0, ListenThread, (LPVOID)dev, 0, NULL );
		CloseHandle ( dev->hListenThread );
	}

	// Send hello to device
	char msg [18];
	memcpy ( msg, g_KeyCR, 16 );
	msg [ 16 ] = 'U';
	msg [ 17 ] = 'A';

	SendMessage ( dev, 18, msg, false );
	//SendMessage ( dev, 18, "Katja", false );

	// Wait for ACK
	if ( !WaitForACK ( dev ) )
		return 0;

	return 1;
}


/*
 * FUNCTION: DisconnectDevice ( )
 *
 * PURPOSE:
 *
 * RETURN:
 *
 */
EXPORT
void DisconnectDevice ( int devNum )
{
	// Get device object
	BTDevice * dev = GetDevice ( devNum );
	if ( !dev )
		return;

	if ( dev->Socket != INVALID_SOCKET )
	{
		closesocket ( dev->Socket );
		dev->Socket = INVALID_SOCKET;
	}

	Sleep ( 10 );
	if ( dev->hListenThread )
	{
		TerminateThread ( dev->hListenThread, -1 );
		dev->hListenThread = NULL;
	}

	if ( dev->hReceiveEvent )
	{
		CloseHandle ( dev->hReceiveEvent );
		dev->hReceiveEvent = NULL;
	}
}



/*
 * FUNCTION: ShutDownCamera ( )
 *
 * PURPOSE:
 *
 * RETURN:
 *
 */
EXPORT
void ShutDownCamera ( int devNum )
{
	// Get device object
	BTDevice * dev = GetDevice ( devNum );
	if ( !dev )
		return;

	// Send hello to device
	char msg [] = {0x01, MSG_CAMERA_SHUTDOWN, 0x00 }; //"    CSD:";

	SendMessage ( dev, strlen(msg), msg );

	// Wait for ACK
	WaitForACK ( dev );
}



/*
 * FUNCTION: UpdateDevices ( )
 *
 * PURPOSE:  Search for bluetooth-devices, check if our service is offered there and update the device list
 *
 * RETURN: -1 means no bluetooth found, 0..n means number of supported devices found
 *
 */
EXPORT
int UpdateDevices ( )
{
	union {
		CHAR buffer [ MAX_BUFF_SIZE ];
		double __unused;
	};
	
	WSAQUERYSET		qset;
    memset ( &qset, 0, sizeof(qset) );
	qset.dwSize			= sizeof(qset);
	qset.dwNameSpace	= NS_BTH;

	HANDLE	hSearch = NULL;
	DWORD	dwFlags = LUP_CONTAINERS | LUP_FLUSHCACHE;
	if ( WSALookupServiceBegin ( &qset, dwFlags, &hSearch ) )
	{
		long lerr = WSAGetLastError();
		return -1;
	}

	LPWSAQUERYSET pResults = (LPWSAQUERYSET) buffer;
	memset ( pResults, 0, MAX_BUFF_SIZE );
	pResults->dwSize		= sizeof(WSAQUERYSET);
	pResults->dwNameSpace	= NS_BTH;
	
	dwFlags = LUP_RETURN_NAME | LUP_RETURN_ADDR;

	DWORD	dwSize;
	LPWSTR	pName;

	// reset stati of devicelist
	BTDevice * dev = g_DeviceList.Start;
	while ( dev )
	{
		if ( !dev->hListenThread )
			dev->Alive = FALSE;
		dev = dev->Next;
	}

	while ( true )
	{
		dwSize  = MAX_BUFF_SIZE;
		if ( WSALookupServiceNext ( hSearch, dwFlags, &dwSize, pResults ) )
			break;

		if ( pResults->dwNumberOfCsAddrs != 1 )
			break;

		BTH_ADDR addr = ( (SOCKADDR_BTH *)pResults->lpcsaBuffer->RemoteAddr.lpSockaddr )->btAddr;
		if ( !addr )
			continue;

		// check if device is already in the list
		dev = g_DeviceList.Start;
		while ( dev )
		{
			if ( dev->Address == addr )
			{
				dev->Alive = TRUE;
				break;
			}
			dev = dev->Next;
		}

		if ( dev )
			continue;

		// Create a new entry
		BTDevice * newDev = new BTDevice;
		if ( !newDev )
			continue;
		memset ( newDev, 0, sizeof(BTDevice) );
		newDev->Address = addr;
		newDev->Alive = TRUE;
		newDev->Socket = INVALID_SOCKET;

		// Attach the entry to our list
		if ( g_DeviceList.Count == 0 )
		{
			g_DeviceList.Start = newDev;
			g_DeviceList.Last = newDev;
		}
		else
		{
			g_DeviceList.Last->Next = newDev;
			g_DeviceList.Last = newDev;
		}

		pName = pResults->lpszServiceInstanceName;
		if ( pName && *pName )
			StringCchCopy ( newDev->Name, MAX_NAME_SIZE, pName );

		if ( LookUpService ( pResults->lpcsaBuffer ) )
		{
			newDev->hasSupport = TRUE;
			g_DeviceList.Count++;
		}
	}
    
	WSALookupServiceEnd ( hSearch );

	RemoveDeadDevices ();

	return g_DeviceList.Count;
}


/*
 * FUNCTION: Release ( )
 *
 * PURPOSE:  Release resources
 *
 */
EXPORT
int GetDeviceList ( int Count, BTDeviceInfo  DeviceInfos[] )
{
	if ( !DeviceInfos )
		return FALSE;

	int			i	= 0;
	BTDevice *	dev = g_DeviceList.Start;

	while ( dev )
	{
		if ( dev->hasSupport )
		{
			DeviceInfos [ i ].Address = dev->Address;
			StringCchCopy ( DeviceInfos[i].Name, MAX_NAME_SIZE, dev->Name );
			i++;

			if ( i == Count )
				return TRUE;
		}
		dev = dev->Next;
	}
	return TRUE;
}