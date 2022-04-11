<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<!--#include virtual="/systemLib/systemcheck.asp"-->
<!--#include virtual="/lib/cn1.asp"-->
<!--#include virtual="systemlib/decode.asp"-->
<%
	path=replace(HTMLreplace(request("path")),".",",")
	if path="" then
		path=0
	end if
	design_style="3"
%>
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
        <meta http-equiv="x-ua-compatible" content="IE=edge, chrome=1" />
        <meta name="apple-mobile-web-app-capable" content="yes">
		<meta name="viewport" content="width=device-width, initial-scale=1.0" />
		<title>雲端網站管理器</title>
		<!-- Bootstrap -->
		<link rel="stylesheet" href="/admin/bootstrap3/css/bootstrap.min.css">
        <link rel="stylesheet" href="/Scripts/jquery-ui-1.11.3/jquery-ui.css">
		<link href="/admin/css.css" rel="stylesheet" type="text/css" />
		<!-- Documentation extras -->
		<link href="/admin/css/docs.css" rel="stylesheet">
		<link href="/admin/css/frame.css" rel="stylesheet">
		<link rel="stylesheet" type="text/css" href="/Scripts/sweetalert/lib/sweet-alert.css">
		<link rel="stylesheet" type="text/css" href="/admin/css/sweet-alert-main.css">
        <link rel="stylesheet" type="text/css" href="/admin/window/view/css/ReleaseSet.css">
		<!-- HTML5 Shim and Respond.js IE8 support of HTML5 elements and media queries -->
		<!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
		<!--[if lt IE 9]>
		<script src="http://cdn.bootcss.com/html5shiv/3.7.0/html5shiv.min.js"></script>
		<script src="http://cdn.bootcss.com/respond.js/1.3.0/respond.min.js"></script>
		<![endif]-->
	</head>
	<body style="padding:0px;">
		<div id="ReleaseSet" class="container">
        	<!--頂端送出按鈕-->
            <div class="row">
            	<div class="col-xs-8 myTitle">
                	請勾選發佈裝置
                </div>
                <div class="col-xs-4" style="padding:0px;">
                    <button type="button" class="btn btn-danger submitbtn pull-right" style="margin:0px;">
                        <i class="glyphicon glyphicon-saved"></i>
                        <span>確認送出</span>
                    </button>
                </div>
            </div>
        	<!--頂端送出按鈕END-->
            <div class="panel-heading row">
                <div class="col-xs-3" data-toggle="collapse" data-target="#group_set" style="cursor:pointer;">
                	<strong style=" font-size:12pt;">群組設定</strong>
                </div>
                <div class="col-xs-6">
                    <button class="btn btn-block" type="button" data-toggle="collapse" data-target="#group_set" aria-expanded="false" aria-controls="collapseExample" style="padding: 0;background: inherit;margin-top: 4px;">
                        <span class="glyphicon glyphicon-chevron-down" aria-hidden="true"></span>
                    </button>
                </div>
                <div class="col-xs-3" style="text-align:right;" data-toggle="allCheck" data-target="group_set">
                	<input type="checkbox" style="margin-top: -3px;margin-right: 5px;vertical-align: middle; display:none;" /><span class="glyphicon glyphicon-ok-sign"></span>&nbsp;發佈於所有群組
                </div>
			</div>
            <div>
                <div class="collapse" id="group_set">
                    <div class="row" cockerAPI="{
                        'rowCont':'4',
                        'rowClass':'row',
                        'colClass':'col-xs-3',
                        'each':{
                        	'type':'text',
                            'titleClass':'itemTitle'
                        }
                    }">
                    	<div class="col-xs-3">
                            <input type="checkbox" /><span class="glyphicon glyphicon-ok-sign"></span>&nbsp;<span class="itemTitle"></span>
                        </div>
                    </div>
                </div>
            </div>
            <div class="panel-heading row">
                <div class="col-xs-3" data-toggle="collapse" data-target="#kiosk_single_set" style="cursor:pointer;">
                	<strong style=" font-size:12pt;">Kiosk播放型廣告機</strong>
                </div>
                <div class="col-xs-6">
                    <button class="btn btn-block" type="button" data-toggle="collapse" data-target="#kiosk_single_set" aria-expanded="false" aria-controls="collapseExample" style="padding: 0;background: inherit;margin-top: 4px;">
                        <span class="glyphicon glyphicon-chevron-down" aria-hidden="true"></span>
                    </button>
				</div>
                <div class="col-xs-3" style="padding-top:3px; text-align:right;"
                	data-toggle="allCheck" data-target="kiosk_single_set">
                	<input type="checkbox" style="margin-top: -3px;margin-right: 5px;vertical-align: middle; display:none;" /><span class="glyphicon glyphicon-ok-sign"></span>&nbsp;發佈於所有裝置
                </div>
			</div>
            <div class="collapse" id="kiosk_single_set">
                <div class="row" cockerAPI="{
                    'rowCont':'4',
                    'rowClass':'row',
                    'colClass':'col-xs-3',
                    'each':{
                    	'type':'text',
                        'titleClass':'itemTitle'
                    }
                }">
                    <div class="col-xs-3">
                        <input class="checkItem" type="checkbox" /><span class="glyphicon glyphicon-ok-sign"></span>&nbsp;<span class="itemTitle"></span>
                    </div>
                </div>
            </div>
            <div class="panel-heading row">
                <div class="col-xs-3" data-toggle="collapse" data-target="#signage_single_set" style="cursor:pointer;">
                	<strong style=" font-size:12pt;">Signage觸控型廣告機</strong>
                </div>
                <div class="col-xs-6">
                    <button class="btn btn-block" type="button" data-toggle="collapse" data-target="#signage_single_set" aria-expanded="false" aria-controls="collapseExample" style="padding: 0;background: inherit;margin-top: 4px;">
                        <span class="glyphicon glyphicon-chevron-down" aria-hidden="true"></span>
                    </button>
                </div>
                <div class="col-xs-3" style="padding-top:3px; text-align:right;"
                	data-toggle="allCheck" data-target="signage_single_set">
                	<input type="checkbox" style="margin-top: -3px;margin-right: 5px;vertical-align: middle; display:none;" /><span class="glyphicon glyphicon-ok-sign"></span>&nbsp;發佈於所有裝置
                </div>
			</div>
            <div class="collapse" id="signage_single_set">
                <div class="row" cockerAPI="{
                    'each':'text',
                    'titleClass':'itemTitle',
                    'rowCont':'4',
                    'rowClass':'row',
                    'colClass':'col-xs-3'
                }">
                    <div class="col-xs-3">
                        <input type="checkbox" class="checkItem" /><span class="glyphicon glyphicon-ok-sign"></span>&nbsp;<span class="itemTitle"></span>
                    </div>
                </div>
            </div>
            <div class="panel-heading row" id="appTag">
                <div class="col-xs-9">
                	<strong>APP</strong><span style=" font-size:10pt; color:#999;"> (僅提供註記)</span>
                </div>
                <div class="col-xs-3" data-toggle="allCheck" style="padding-top:3px; text-align:right;">
                	<input type="checkbox" style="margin-top: -3px;margin-right: 5px;vertical-align: middle; display:none;" /><span class="glyphicon glyphicon-ok-sign"></span>&nbsp;已發佈
                </div>
			</div>
        </div>
		<!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
		<script src="/js/jquery-1.10.2.min.js"></script>
        <script src="/js/cockerAPI/CockerAPI.js"></script>
		<script src="/Scripts/sweetalert/lib/sweet-alert.js"></script>
		<!-- Include all compiled plugins (below), or include individual files as needed -->
		<script src="/admin/bootstrap3/js/bootstrap.min.js"></script>
        <script src="/js/data/UrlParameter.js"></script>
		<script src="/admin/js/application.js"></script>
		<script src="/js/knockout/knockout-3.0.0.debug.js"></script>
		<script type="text/javascript" src="/ckeditor/ckeditor.js"></script>
		<script type="text/javascript" src="/admin/js/templateLoad.js"></script>
		<script src="/admin/js/basicCtrl.js"></script>
		<!-- Include all compiled plugins (below), or include individual files as needed -->
		<script type="text/javascript" src="/admin/window/control/InputItem.js"></script>
		<script type="text/javascript" src="/admin/window/control/MenuItem.js"></script>
		<script src="/js/autoheight.js"></script>
        <script language="javascript">
			var menuId=<%=request("id")%>;
			var saveGroupSet=new Array();
			var saveKioskSet=new Array();
			var saveSignageSet=new Array();
        	$(document).ready(function(e) {
				$.post("/admin/ashx/Device.ashx",{
					Type:"searchGroup",
					GroupID:"",
					Orgname:"<%=session("orgname")%>"
				},function(groups){
					var group=JSON.parse(groups).Items;
					$.post("/admin/ashx/Device.ashx",{
						Type:"searchDevice",
						VerCode:"",
						Orgname:"<%=session("orgname")%>"
					},function(devices){
						var totalDev=JSON.parse(devices).Items;
						$.post("/admin/ashx/Device.ashx",{
							Type:"searchDevice3",
							MenuID:menuId,
							Orgname:"<%=session("orgname")%>"
						},function(data){
							var obj=JSON.parse(data).Items[0];
							var kioskDevice=new Array();
							var signageDevice=new Array();
							var kioskAllDevice=new Array();
							var signageAllDevice=new Array();
							if(obj.AppTag=="Y"){
								$("#appTag [data-toggle='allCheck']")
									.addClass("selected")
									.find("input[type='checkbox']").prop("checked",true);
							}
							obj.Device.forEach(function(item){
								if(parseInt(item.Type)==3) kioskDevice.push(item);
								else if(parseInt(item.Type)==4) signageDevice.push(item);
							});
							totalDev.forEach(function(item){
								if(parseInt(item.Type)==3) kioskAllDevice.push(item);
								else if(parseInt(item.Type)==4) signageAllDevice.push(item);
							});
							new CockerAPI({
								box:$("#group_set"),
								data:group,
								checkSet:obj.Group,
								checkKey:"GroupID",
								title:"GroupName",
								attr:["GroupID","GroupName"],
								event:{
									click:selected,
								}
							});
							new CockerAPI({
								box:$("#kiosk_single_set"),
								data:kioskAllDevice,
								checkSet:kioskDevice,
								checkKey:"VerCode",
								title:"Title",
								attr:["Title","Type","Stat","VerCode","MachineCode","GroupID"],
								event:{
									click:selected,
								}
							});
							new CockerAPI({
								box:$("#signage_single_set"),
								data:signageAllDevice,
								checkSet:signageDevice,
								checkKey:"VerCode",
								title:"Title",
								attr:["Title","Type","Stat","VerCode","MachineCode","GroupID"],
								event:{
									click:selected,
								}
							});
						});
					});
				});
				$(".submitbtn").unbind("click").click(function(){
					var obj={"Items": []};
					saveKioskSet.forEach(function(item){
						var canView=$(item).hasClass("selected")?"Y":"N";
						var itemData={
							"GroupID":"",
							"VerCode":$(item).attr("verCode"),
							"Authority":[{
								"MenuID":menuId,
								"Type":"menu_sub",
								"CanView":canView,
								"CanEdit":canView,
								"CanDel":canView,
								"CanAdd":canView
							}]
						};
						obj.Items.push(itemData);
					});
					saveSignageSet.forEach(function(item){
						var canView=$(item).hasClass("selected")?"Y":"N";
						var itemData={
							"GroupID":"",
							"VerCode":$(item).attr("verCode"),
							"Authority":[{
								"MenuID":menuId,
								"Type":"menu_sub",
								"CanView":canView,
								"CanEdit":canView,
								"CanDel":canView,
								"CanAdd":canView
							}]
						};
						obj.Items.push(itemData);
					});
					$.post("/admin/ashx/Device.ashx",{
						Type:"editAuthority",
						Orgname:"<%=session("orgname")%>",
						ItemData:JSON.stringify(obj)
					},function(result){
						obj=null;
						obj={"Items": []};
						saveGroupSet.forEach(function(item){
							var canView=$(item).hasClass("selected")?"Y":"N";
							var itemData={
								"GroupID":$(item).attr("groupid"),
								"VerCode":"",
								"Authority":[{
									"MenuID":menuId,
									"Type":"menu_sub",
									"CanView":canView,
									"CanEdit":canView,
									"CanDel":canView,
									"CanAdd":canView
								}]
							};
							obj.Items.push(itemData);
						});
						$.post("/admin/ashx/Device.ashx",{
							Type:"editAuthority",
							Orgname:"<%=session("orgname")%>",
							ItemData:JSON.stringify(obj)
						},function(result2){
							var appTag=$("#appTag [data-toggle='allCheck']").hasClass("selected")?"Y":"N";
							$.post("/admin/ashx/Device.ashx",{
								Type:"apptag",
								Orgname:"<%=session("orgname")%>",
								MenuID:menuId,
								Tag:appTag
							},function(result3){
								if(result3.indexOf("success")!=-1) sAlert("儲存成功");
							});
						});
					});
				});
            });
			var selected=function(item){
				if($(item).find("input[type='checkBox']").prop("checked"))
					$(item).removeClass("selected").find("input[type='checkBox']").prop("checked", false);
				else
					$(item).addClass("selected").find("input[type='checkBox']").prop("checked", true);
				if($(item).attr("data-target")=="group_set"){
					saveGroupSet=null;
					saveGroupSet=new Array();
					if($(item).hasClass("selected")){
						$("#"+$(item).attr("data-target")).find("input[type='checkBox']").each(function(index, element) {
							if(typeof($(element).attr("initcheck"))!="undefined"){
	                            saveGroupSet.push($(element).parent(".selected")[0]);
							}
                        });
					}
				}else if($(item).attr("data-target")=="kiosk_single_set"){
					saveKioskSet=null;
					saveKioskSet=new Array();
					console.log($(item)[0]);
					if($(item).hasClass("selected")){
						$("#"+$(item).attr("data-target")).find("input[type='checkBox']").each(function(index, element) {
							if(typeof($(element).attr("initcheck"))!="undefined"){
	                            saveKioskSet.push($(element).parent(".selected")[0]);
							}
                        });
					}
					saveSignageSet=null;
					saveSignageSet=new Array();
					$('[data-target="signage_single_set"]').removeClass("selected");
					$('[data-target="signage_single_set"]').find("input[type='checkBox']").prop("checked",false);
					$("#signage_single_set").find(".selected").removeClass("selected");
					$("#signage_single_set").find("input[type='checkBox']").prop("checked",false);
				}else if($(item).attr("data-target")=="signage_single_set"){
					saveSignageSet=null;
					saveSignageSet=new Array();
					if($(item).hasClass("selected")){
						$("#"+$(item).attr("data-target")).find("input[type='checkBox']").each(function(index, element) {
							if(typeof($(element).attr("initcheck"))!="undefined"){
	                            saveSignageSet.push($(element).parent(".selected")[0]);
							}
                        });
					}
					saveKioskSet=null;
					saveKioskSet=new Array();
					$('[data-target="kiosk_single_set"]').removeClass("selected");
					$('[data-target="kiosk_single_set"]').find("input[type='checkBox']").prop("checked",false);
					$("#kiosk_single_set").removeClass("selected").find(".selected").removeClass("selected");
					$("#kiosk_single_set").find("input[type='checkBox']").prop("checked",false);
				}else if($(item).parents(".collapse").attr("id")=="group_set"){
					if($.inArray( item, saveGroupSet )==-1) saveGroupSet.push(item);
					var $index=$.inArray( item, saveGroupSet );
					if($index!=-1){
						var initCheck = $(saveGroupSet[$index]).find("input[type='checkBox']")
															   .attr("initcheck")=="true"?true:false;
						if(initCheck==$(item).hasClass("selected")) saveGroupSet.splice($index,1);
					}
				}else if($(item).parents(".collapse").attr("id")=="kiosk_single_set"){
					if($.inArray( item, saveKioskSet )==-1) saveKioskSet.push(item);
					var $index=$.inArray( item, saveKioskSet );
					if($index!=-1){
						var initCheck = $(saveKioskSet[$index]).find("input[type='checkBox']")
															   .attr("initcheck")=="true"?true:false;
						if(initCheck==$(item).hasClass("selected")) saveKioskSet.splice($index,1);
						if($(item).hasClass("selected")){
							var disItem=$('#signage_single_set [VerCode="'+$(item).attr("VerCode")+'"]');
							$(disItem).removeClass("selected");
							$(disItem).find("input[type='checkBox']").prop("checked",false);
						}
					}
				}else if($(item).parents(".collapse").attr("id")=="signage_single_set"){
					if($.inArray( item, saveSignageSet )==-1) saveSignageSet.push(item);
					var $index=$.inArray( item, saveSignageSet );
					if($index!=-1){
						var initCheck = $(saveSignageSet[$index]).find("input[type='checkBox']")
															   .attr("initcheck")=="true"?true:false;
						if(initCheck==$(item).hasClass("selected")) saveSignageSet.splice($index,1);
						if($(item).hasClass("selected")){
							var disItem=$('#kiosk_single_set [VerCode="'+$(item).attr("VerCode")+'"]');
							$(disItem).removeClass("selected");
							$(disItem).find("input[type='checkBox']").prop("checked",false);
						}
					}
				}
			}
			$('[data-toggle="collapse"]').click(function(){
				showAnimeIframe();
			});
			$('[data-toggle="allCheck"]').click(function(){
				var $check=!$(this).find('input[type="checkBox"]').prop("checked");
				if($check){
					var element=$("#"+$(this).attr("data-target")).find('input[type="checkBox"]');
					$(element).prop("checked",true);
					$(element).parent(".col-xs-3").addClass("selected")
				}else{
					$("#"+$(this).attr("data-target")).find('input[type="checkBox"]').each(function(index, element) {
						var initCheck=$(element).attr("initCheck")=="true"?true:false;
                        $(element).prop("checked",initCheck);
						if(!initCheck)
							$(element).parent(".col-xs-3").removeClass("selected")
                    });
				}
				selected(this);
			});
        </script>
	</body>
</html>
