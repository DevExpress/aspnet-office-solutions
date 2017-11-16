echo Because all HTTP requests and responses for the content sites go through ARR, the worker process for the Default Web Site should always be running.
FOR /F "tokens=* delims=" %%i in ('%systemroot%\system32\inetsrv\appcmd list apppool /text:name') do ( 
	echo setting up the apppool: "%%i"
	%systemroot%\system32\inetsrv\APPCMD set apppool "%%i" /enable32BitAppOnWin64:true
)