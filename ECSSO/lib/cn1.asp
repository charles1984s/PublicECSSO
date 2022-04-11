<%
Response.Buffer=true
Response.Expires = -1
Response.AddHeader "Pragma", "no-cache"
Response.AddHeader "cache-control", "no-store"


CNSTR="Provider=SQLOLEDB.1;User ID=i_template_"&session("orgname")&";Initial Catalog=template_"&session("orgname")&";Data Source=."
CNTimeOut = 15
CMTimeOut = 30
CRLoc = 3
usrname = "i_template_"&session("orgname")
USRpwd = "i_template_"&session("orgname")&"1234"

Set cn1 = Server.CreateObject("ADODB.Connection")
cn1.ConnectionTimeout=CNTimeOut
cn1.CommandTimeout=CMTimeOut
cn1.Open CNSTR,usrname,USRpwd
sub debugSQL(sql)
   response.write sql
   response.end
end sub
%>