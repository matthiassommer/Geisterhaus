using System;
using System.IO;

namespace Geisterhaus
{
    class textdatei
    {
        public string ReadLine(int iLine)
        {
            string sContent = "";
            float fRow = 0;
            if (File.Exists("highscore.txt"))
            {
                StreamReader myFile = new StreamReader("highscore.txt", System.Text.Encoding.Default);
                while (!myFile.EndOfStream && fRow < iLine)
                {
                    fRow++;
                    sContent = myFile.ReadLine();
                }
                myFile.Close();
                if (fRow < iLine)
                    sContent = "";
            }
            return sContent;
        }

        public void WriteLine(int iLine, string sLines)
        {
            string sContent = "";
            string[] delimiterstring = { "\r\n" };

            if (File.Exists("highscore.txt"))
            {
                StreamReader myFile = new StreamReader("highscore.txt", System.Text.Encoding.Default);
                sContent = myFile.ReadToEnd();
                myFile.Close();
            }

            string[] sCols = sContent.Split(delimiterstring, StringSplitOptions.None);

            if (sCols.Length >= iLine)
            {
                sCols[iLine - 1] = sLines;

                sContent = "";
                for (int x = 0; x < sCols.Length - 1; x++)
                {
                    sContent += sCols[x] + "\r\n";
                }
                sContent += sCols[sCols.Length - 1];

            }
            else
            {
                for (int x = 0; x < iLine - sCols.Length; x++)
                    sContent += "\r\n";

                sContent += sLines;
            }

            StreamWriter mySaveFile = new StreamWriter("highscore.txt");
            mySaveFile.Write(sContent);
            mySaveFile.Close();
        }
    }
}
