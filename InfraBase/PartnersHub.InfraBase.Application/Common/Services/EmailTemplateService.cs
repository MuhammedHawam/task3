namespace PartnersHub.InfraBase.Application.Common.Services;

public class EmailTemplateService
{
    public string BuildAssetSubmittedEmail(string creatorName, Guid assetId)
    {
        var assetDetailsUrl = $"/assets/{assetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New asset submitted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #007bff; margin: 0;"">New asset submitted</h1>
        </div>
        <div class=""content"">
            <p>New asset submitted by ""{creatorName}"" and waiting your approval</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildAssetAcceptedByPcAdminEmail(Guid assetId)
    {
        var assetDetailsUrl = $"/assets/{assetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Accepted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #28a745;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #28a745; margin: 0;"">Asset Accepted</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Approved</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildAssetRejectedByPcAdminEmail(Guid assetId)
    {
        var assetDetailsUrl = $"/assets/{assetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Rejected</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #dc3545;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #dc3545; margin: 0;"">Asset Rejected</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Rejected</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildNewRequestSubmittedEmail(string companyName, Guid assetId)
    {
        var assetDetailsUrl = $"/assets/{assetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New request submitted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #007bff; margin: 0;"">New request submitted</h1>
        </div>
        <div class=""content"">
            <p>New request submitted by ""{companyName}"" and waiting your approval</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">Approve Request</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildAssetAcceptedByInfrabaseAdminEmail(Guid assetId)
    {
        var assetDetailsUrl = $"/assets/{assetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Accepted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #28a745;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #28a745; margin: 0;"">Asset Accepted</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Approved</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildAssetRejectedByInfrabaseAdminEmail(Guid assetId)
    {
        var assetDetailsUrl = $"/assets/{assetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Rejected</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #dc3545;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #dc3545; margin: 0;"">Asset Rejected</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Rejected</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }
}
