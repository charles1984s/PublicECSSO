<%
Response.Buffer=true
Response.Expires = -1
Response.AddHeader "Pragma", "no-cache"
Response.AddHeader "cache-control", "no-store"


'連結sqlserver
'*************要連到mart's shop的DB
Set cn2 = Server.CreateObject("ADODB.Connection")
cn2.ConnectionTimeout=application("asia_ConnectionTimeout")
cn2.CommandTimeout=application("asia_CommandTimeout")
cn2.Open application("cn2_connectionString"),application("cn2_RuntimeUserName"),application("cn2_RuntimePassword")
sub debugSQL(sql)
   response.write sql
   response.end
end sub
%>