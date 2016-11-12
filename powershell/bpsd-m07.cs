using System;

public class BPSDObjectExternal
{
  public static string composeFullName(string pSchema, string pTable)
  {
    string retVal = "";  

    retVal = pSchema + "." + pTable;

    return retVal;

  } // public static string composeFullName
} // class BPSDObjectExternal
