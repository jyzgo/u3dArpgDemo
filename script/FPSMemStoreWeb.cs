using UnityEngine;
using System.Collections;
using System.Net;
using System.Collections.Specialized;
using System;
using System.Text;

public class FPSMemStoreWeb : FPSMemStoreAbstract
{
    public string WebStoreAddress;
    
    public override void Store(FPSMemRecord[] records)
    {
        foreach(FPSMemRecord record in records)
            Store(record);
    }
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////
    
    private void Store(FPSMemRecord record)
    {
        CreateWebClient().UploadValuesAsync(
            new Uri(WebStoreAddress),
            "POST",
            CreateParams(record),
            Guid.NewGuid().ToString());
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////
    
    private WebClient CreateWebClient()
    {
        var webClient = new WebClient();
        return webClient;
    }
    
    private NameValueCollection CreateParams(FPSMemRecord record)
    {
        var keyVal = new NameValueCollection();
        
        foreach(var pair in record.Data())
            keyVal.Add(pair.Key, pair.Value.ToString());
    
        return keyVal;
    }
}
