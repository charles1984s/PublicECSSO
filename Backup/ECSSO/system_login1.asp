
<!--#include virtual="/lib/cn2.asp"-->

<%
session("Crm_version")=REQUEST("Crm_version")
session("systemloginid")=REQUEST("username")
session("systemloginpwd")=REQUEST("password")
session("orgname")=request("orgname")
%>
<!--#include virtual="/lib/cn1.asp"-->
<%

set cmd = server.createobject("Adodb.command")
With cmd
	.ActiveConnection = cn2
	.CommandType=1
	.NamedParameters = True
	.CommandText = "select * from IDManagement where orgName=? and manager_id=?"	
	.Parameters.Append .CreateParameter("@orgName",200,1,50,session("orgname"))
	.Parameters.Append .CreateParameter("@manager_id",200,1,50,session("systemloginid"))
end with
set rslog = cmd.execute
if rslog.eof then
	response.write "<script type='text/javascript'>alert('login error'); window.location.href='Manage.aspx';</script>"
else


    set cmd2 = server.createobject("Adodb.command")  
	With cmd2
		.ActiveConnection = cn2
		.CommandType=1
		.NamedParameters = True
		.CommandText = "select web_url from cocker_cust where comp_en_name=?"	
		.Parameters.Append .CreateParameter("@comp_en_name",200,1,50,session("orgname"))
	end with
	set rsweb = cmd2.execute
    if not rsweb.eof then
        session("weburl")=rsweb(0)
    end if
    Set cmd2 = Nothing
	
	
	set cmd2 = server.createobject("Adodb.command")
	With cmd2
		.ActiveConnection = cn1
		.CommandType=1
		.NamedParameters = True
		.CommandText = "select * from empl where empl_id=?"	
		.Parameters.Append .CreateParameter("@empl_id",200,1,50,session("systemloginid"))
	end with
	set rs=cmd2.execute
	Set cmd2 = Nothing
	if rs.eof then
		session("manager")="Y"
	else

		session("manager")=rs("manager")
	end if
	Set cmd2 = Nothing
	
	
	set cmd2 = server.createobject("Adodb.command")
	With cmd2
		.ActiveConnection = cn1
		.CommandType=1
		.NamedParameters = True
		.CommandText = "select system_type from head"
	end with
	set rs=cmd2.execute
	Set cmd2 = Nothing
	if not rs.eof then
		if rs("system_type")="Ooads" then
			session("system_type")="Ooads"
		end if
	end if
	if session("system_type")="" then
		session("system_type")="cocker"
	end if
	
	
    set cmd2 = server.createobject("Adodb.command")  
    With cmd2
		.ActiveConnection = cn1
		.CommandType=1
		.NamedParameters = True
		.CommandText = "select a.job_id,b.job_name,b.job_url,a.canexe from authors as a left join webjobs as b on a.job_id=b.job_id where empl_id=? order by a.job_id"	
		.Parameters.Append .CreateParameter("@empl_id",200,1,50,session("systemloginid"))
	end with
	
	set rsPower=cmd2.execute
	Set cmd2 = Nothing
	
	Dim lmenu_url(5)
	Dim job_name(5)
	
	while not rsPower.eof
		select case left(rsPower(0),1)
			case "A"
				if rsPower(3)="Y" then
					lmenu_url(0)=lmenu_url(0)&","&rsPower(2)&"?job_id="&rsPower(0)
				else
					lmenu_url(0)=lmenu_url(0)&","
				end if
				job_name(0)=job_name(0)&","&rsPower(1)
			case "C"
				if rsPower(3)="Y" then
					lmenu_url(1)=lmenu_url(1)&","&rsPower(2)&"?job_id="&rsPower(0)
				else
					lmenu_url(1)=lmenu_url(1)&","
				end if
				job_name(1)=job_name(1)&","&rsPower(1)
			case "E"
				if rsPower(3)="Y" then
					lmenu_url(2)=lmenu_url(2)&","&rsPower(2)&"?job_id="&rsPower(0)
				else
					lmenu_url(2)=lmenu_url(2)&","
				end if			
				job_name(2)=job_name(2)&","&rsPower(1)
			case "F"
				if rsPower(3)="Y" then
					lmenu_url(3)=lmenu_url(3)&","&rsPower(2)&"?job_id="&rsPower(0)
				else
					lmenu_url(3)=lmenu_url(3)&","
				end if
				job_name(3)=job_name(3)&","&rsPower(1)
			case "P","Z"
				if rsPower(3)="Y" then
					lmenu_url(4)=lmenu_url(4)&","&rsPower(2)&"?job_id="&rsPower(0)
				else
					lmenu_url(4)=lmenu_url(4)&","
				end if
				job_name(4)=job_name(4)&","&rsPower(1)
		end select
		rsPower.movenext
	wend
	session("lmenu_url")=lmenu_url
	session("job_name")=job_name
	
	
		RESPONSE.REDIRECT "admin/admin01.asp"
end if
Set cmd = Nothing
%>
  