using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public static class TimeUtils 
{
	public static readonly DateTime k_epoch_time = new DateTime(1970, 1, 1, 0, 0, 0);

	public static bool IsBetweenTime(DateTime currentTime, string beginTime, string endTime)
	{
		DateTime visibleStartTime = TimeUtils.ConvertFromStringWithSplit(beginTime, ",");
		DateTime visibleEndTime  = TimeUtils.ConvertFromStringWithSplit(endTime, ",");
		
		if(currentTime.CompareTo(visibleStartTime) >= 0
			&& currentTime.CompareTo(visibleEndTime) <= 0)
		{
			return true;
		}
		return false;
	}
	
	
	public static bool IsBetweenTime(DateTime beginTime, DateTime endTime)
	{
		DateTime currentTime = GetPSTDateTime();
		
		if(currentTime.CompareTo(beginTime) >= 0
			&& currentTime.CompareTo(endTime) <= 0)
		{
			return true;
		}
		return false;
	}
	
	
	
	public static DateTime ConvertFromStringWithSplit(string timeStr, string split)
	{
		string[] splitedTimeStrs = timeStr.Split(split.ToCharArray());
		if(splitedTimeStrs.Length != 6)
		{
			Debug.LogError("wrong time string format or split");	
		}
		
		int year = Convert.ToInt32(splitedTimeStrs[0]);
		int month = Convert.ToInt32(splitedTimeStrs[1]);
		int day = Convert.ToInt32(splitedTimeStrs[2]);
		int hour = Convert.ToInt32(splitedTimeStrs[3]);
		int minute = Convert.ToInt32(splitedTimeStrs[4]);
		int second = Convert.ToInt32(splitedTimeStrs[5]);
		DateTime dt = new DateTime(year, month, day, hour, minute, second);
		
		return dt;
	}
	
	public static DateTime GetPSTDateTime()
	{
		return NetworkManager.Instance.serverTime;
	}
	
	
	public static string GetStandardTimeString(TimeSpan span)
	{
		
		string timeStr = "";
        string strDay = Localization.instance.Get("IDS_MESSAGE_GLOBAL_DAY");
        string strDays = Localization.instance.Get("IDS_MESSAGE_GLOBAL_DAY");
		if(span.Days > 0)
		{
			if(span.Days > 1)
			{
				timeStr = span.Days.ToString() + strDays;
			}
			else
			{
				timeStr = span.Days.ToString() + strDay;
			}
		}
		else if (span.Hours == 0)
		{
			timeStr = string.Format("{0:00}:{1:00}", span.Minutes, span.Seconds);
		}
		else
		{
			timeStr =  string.Format("{0:00}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
		}	
		
		return timeStr;
	}
	
	
	public static string GetLocalizedTimeString(TimeSpan span)
	{
		string timeStr = "";
        string strDay = Localization.instance.Get("IDS_MESSAGE_GLOBAL_DAY");
        string strDays = Localization.instance.Get("IDS_MESSAGE_GLOBAL_DAY");
        string strHour = Localization.instance.Get("IDS_MESSAGE_GLOBAL_HOUR");
        string strMinute = Localization.instance.Get("IDS_MESSAGE_GLOBAL_MINUTE");
		if(span.Days > 0)
		{
			if(span.Days > 1)
			{
				timeStr = span.Days.ToString() + strDays;
			}
			else
			{
				timeStr = span.Days.ToString() + strDay;
			}
		}
		else if(span.Hours > 0)
		{
			timeStr = span.Hours.ToString() + strHour + " " + span.Minutes.ToString() + strMinute  + " " + span.Seconds.ToString();
		}
		else if(span.Minutes > 0)
		{
			timeStr = span.Minutes.ToString() + strMinute + " " + span.Seconds.ToString();
		}
		else if(span.Seconds > 0)
		{
			timeStr = span.Seconds.ToString();
		}
			
		return timeStr;
	}
	
	public static string GetSurplusTimeString(System.DateTime endTime)
	{
		System.TimeSpan span = endTime.Subtract(GetPSTDateTime());
		
		string timeStr = "";
        string strDay = Localization.instance.Get("IDS_MESSAGE_GLOBAL_DAY");
        string strHour = Localization.instance.Get("IDS_MESSAGE_GLOBAL_HOUR");
        string strMinute = Localization.instance.Get("IDS_MESSAGE_GLOBAL_MINUTE");
		
		if(span.Days > 0)
		{
			timeStr = span.Days.ToString() + strDay;
		}
		else if(span.Hours > 0)
		{
			timeStr = span.Hours.ToString() + strHour;
		}
		else if(span.Minutes > 0)
		{
			timeStr = span.Minutes.ToString() + strMinute;
		}
		else if(span.Seconds > 0)
		{
			timeStr = "<1" + strMinute;
		}
			
		return timeStr;
	}
	
	public static string GetTimeStringForDoubleCoinBuff(TimeSpan span)
	{
		int hour = span.Days * 24 + span.Hours;
		int minute = span.Minutes;
		int second = span.Seconds;
		
		string timeStr = "";
		if(hour > 0)
		{
			timeStr += hour.ToString() + ":";
			if(minute < 10)
			{
				timeStr += "0";	
			}
			timeStr += minute.ToString();
			return timeStr;
		}
		else if(minute > 0)
		{
			timeStr += minute.ToString() + ":";
			if(second < 10)
			{
				timeStr += "0";	
			}
			timeStr += second.ToString();
			return timeStr;
		}
		else
		{
			timeStr += "00:";
			if(second < 10)
			{
				timeStr += "0";	
			}
			timeStr += second.ToString();
			return timeStr;
		}
		
	}
	
	public static DateTime ConvertFromUnixTimestamp(double timestamp)
	{
	    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddSeconds(timestamp);
	}
	
	public static double ConvertToUnixTimestamp(DateTime date)
	{
	    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
	    TimeSpan diff = date.ToLocalTime() - origin;
	    return Math.Floor(diff.TotalSeconds);
	}
         
	
	
}
