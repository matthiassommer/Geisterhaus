#pragma once


// Include for WinSock 2.2
#include "winsock2.h"

//#include "Bthsdpdef.h"
//#include "BluetoothAPIs.h"
#include <ws2bth.h>
#include <BluetoothAPIs.h>
#include "MessageDefs.h"

#include <strsafe.h>

#define MAX_NAME_SIZE 128
#define MAX_ADDR_SIZE 15
#define MAX_BUFF_SIZE 16384
#define MAX_MESSAGE_SIZE 256

#define EXPORT extern "C" __declspec(dllexport)

struct BTDevice
{
	BOOL			Alive;
	BTH_ADDR		Address;
	TCHAR			Name [ MAX_NAME_SIZE ];
	BOOL			hasSupport;
	BOOL			ACK;
	unsigned char	byte;
	HANDLE			hListenThread;
	HANDLE			hReceiveEvent;
	SOCKET			Socket;
	SOCKADDR_BTH	SocketAddress;
	BTDevice	*	Next;
};

struct BTDeviceInfo
{
	BTH_ADDR		Address;
	TCHAR			Name [ MAX_NAME_SIZE ];
};
