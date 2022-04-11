<%
Response.Buffer=true
Response.Expires = -1
Response.AddHeader "Pragma", "no-cache"
Response.AddHeader "cache-control", "no-store"


'連結sqlserver
'*************要連到mart's shop的DB
CNSTR="Provider=SQLOLEDB.1;User ID=Cocker_Admin;Initial Catalog=Cocker_Admin;Data Source=."
CNTimeOut = 15
CMTimeOut = 30
CRLoc = 3
usrname = "Cocker_Admin"
USRpwd = "Cocker_Admin@135"

Set cn2 = Server.CreateObject("ADODB.Connection")
cn2.ConnectionTimeout=CNTimeOut
cn2.CommandTimeout=CMTimeOut
cn2.Open CNSTR,usrname,USRpwd
sub debugSQL(sql)
   response.write sql
   response.end
end sub
%>