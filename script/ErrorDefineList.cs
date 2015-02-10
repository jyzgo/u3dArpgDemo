using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ErrorDefine
{
    public ErrorDefine(int code, string ids)
    {
        errorCode = code;
        errorIds = ids;
    }

    public int errorCode;

    public string errorIds;

    public string ToLocalization()
    {
        return Localization.Localize(errorIds);
    }
}

public class ErrorDefineList : ScriptableObject
{

    public List<ErrorDefine> errors = new List<ErrorDefine>();

    private Dictionary<int, ErrorDefine> _mapping;
    public Dictionary<int, ErrorDefine> ErrorDefineMapping
    { 
        get
        {
            if (null == _mapping)
            {
                List<ErrorDefine> errorDefineList = new List<ErrorDefine>()
                {
                    { new ErrorDefine((int)FaustComm.ErrorType.www_error,				"IDS_MESSAGE_GLOBAL_NETWORKERROR")},			//www error
		            { new ErrorDefine((int)FaustComm.ErrorType.time_out,				"IDS_MESSAGE_GLOBAL_TIMEOUT")},				//request time out
		            { new ErrorDefine((int)FaustComm.ErrorType.decode_error,			"IDS_MESSAGE_GLOBAL_DECODE_ERROR")},			//client decode error
		            { new ErrorDefine((int)FaustComm.ErrorType.no_return_value,			"IDS_MESSAGE_GLOBAL_NO_RETURN_VALUE")},		//www returns 0 bytes
		            { new ErrorDefine((int)FaustComm.ErrorType.session_timeout,			"IDS_MESSAGE_GLOBAL_SESSION_TIMEOUT")},		//no activity for more than 1 hour
		            { new ErrorDefine((int)FaustComm.ErrorType.session_error,			"IDS_MESSAGE_GLOBAL_SESSION_ERROR")},		//same account has logged in from another device, or for client has sent a wrong session id
		            { new ErrorDefine((int)FaustComm.ErrorType.server_decode_error,		"IDS_MESSAGE_GLOBAL_SERVER_DECODE_ERROR")},	//server cannot decode client data
		            { new ErrorDefine((int)FaustComm.ErrorType.server_wrong_protocol,	"IDS_MESSAGE_GLOBAL_SERVER_WRONG_PROTOCOL")},//server cannot find the specified protocol id
		            { new ErrorDefine((int)FaustComm.ErrorType.server_unknown_error,	"IDS_MESSAGE_GLOBAL_SERVER_UNKNOWN_ERROR")} //unknown server error, see error msg for details
                };
                errorDefineList.AddRange(errors);

                _mapping = new Dictionary<int, ErrorDefine>();
                Dictionary<int, ErrorDefine> mapping = new Dictionary<int, ErrorDefine>();
                foreach (ErrorDefine error in errorDefineList)
                {
                    _mapping.Add(error.errorCode, error);
                }
            }
            return _mapping;
        }
    }
}
