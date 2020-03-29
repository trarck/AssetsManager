var http=require("http"),
    url=require("url"),
    fs=require("fs"),
    path=require("path");

var mine=require("./mine");

//var HOST=null,
//    PORT=8111,
//    WEBROOT="./";
//
//var RootMaps={
//    "/yhge.js":"D:/project/trarck/game/yhge/build/dist/0.11/yhge.js"
//};
//
//var httpServer=http.createServer(function (request,response) {
//    var pathname = url.parse(request.url).pathname;
//    var realPath=RootMaps[pathname]||path.join(WEBROOT,pathname);
//    console.log(request.method+" "+request.url +"("+pathname+","+realPath+")");
//    path.exists(realPath,function (exists) {
//        if(exists){
//           //file or dir
//           fs.stat(realPath,function (err,stats) {
//                if(err){
//                    response.writeHead(500,{"Content-type":"text/plain"});
//                    response.end();
//                }else{
//                    if(stats.isFile()){
//                        fs.readFile(realPath,"binary",function (err,file) {
//                            if(err){
//                                response.writeHead(500,{"Content-type":"text/plain"});
//                                response.end();
//                            }else{
//                                var ext=path.extname(realPath);
//                                var contentType=mine.lookupExtension(ext);
//                                response.writeHead(200,{"Content-type":contentType});
//                                response.write(file,"binary");
//                                response.end();
//                            }
//                        });
//                    }else if(stats.isDirectory()){
//                        fs.readdir(realPath,function (err,files) {
//                            if(err){
//                                response.writeHead(500,{"Content-type":"text/plain"});
//                                response.end();
//                            }else{
//                                html="<html><body>";
//                                html+="<ul><li><a href='./'>.</a></li><li><a href='../'>..</a></li>";
//                                for(var i in files){
//                                    var filePath=path.join(realPath,files[i]);
//                                    var fileStats=fs.statSync(filePath);
//                                    if(fileStats.isFile()){
//                                        html+="<li><a href='"+files[i]+"'>"+files[i]+"</a></li>";
//                                    }else{
//                                        html+="<li><a href='"+files[i]+"/'>"+files[i]+"</a></li>";
//                                    }
//                                }
//                                html+="</ul></body></html>";
//                                response.writeHead(200,{"Content-type":"text/html"});
//                                response.write(html);
//                                response.end();
//                            }
//                        });
//                    }
//                }
//           });
//           
//        }else{
//            response.writeHead(404,{"Content-type":"text/plain"});
//            response.write("The request url "+pathname+" not found!");
//            response.end(); 
//        }
//    });
//   
//});
//httpServer.listen(PORT,HOST);

exports.createServer=function (port,host,webRoot,routeMaps) {
    routeMaps=routeMaps||{};//TODO default route maps
    var httpServer=http.createServer(function (request,response) {
        var requestUrl=decodeURIComponent(request.url);
        var pathname = url.parse(requestUrl).pathname;
        // var realPath=routeMaps[pathname]||path.join(webRoot,pathname);
		var realPath=getRealPath(pathname,webRoot,routeMaps);
        console.log(request.method+" "+requestUrl +"("+pathname+","+realPath+")");
		realPath=path.normalize(realPath);
        fs.exists(realPath,function (exists) {
            if(exists){
               //file or dir
               fs.stat(realPath,function (err,stats) {
                    if(err){
                        response.writeHead(500,{"Content-type":"text/plain"});
                        response.end();
                    }else{
                        if(stats.isFile()){
                            fs.readFile(realPath,"binary",function (err,file) {
                                if(err){
                                    response.writeHead(500,{"Content-type":"text/plain"});
                                    response.end();
                                }else{
                                    var ext=path.extname(realPath);
                                    var contentType=mine.lookupExtension(ext);
                                    response.writeHead(200,{"Content-type":contentType});
                                    response.write(file,"binary");
                                    response.end();
                                }
                            });
                        }else if(stats.isDirectory()){
                            fs.readdir(realPath,function (err,files) {
                                if(err){
                                    response.writeHead(500,{"Content-type":"text/plain"});
                                    response.end();
                                }else{
                                    html='<html><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/></head><body>';
                                    html+="<ul><li><a href='./'>.</a></li><li><a href='../'>..</a></li>";
                                    for(var i in files){
                                        var filePath=path.join(realPath,files[i]);
                                        var fileStats=fs.statSync(filePath);
                                        if(fileStats.isFile()){
                                            html+="<li><a href='"+files[i]+"'>"+files[i]+"</a></li>";
                                        }else{
                                            html+="<li><a href='"+files[i]+"/'>"+files[i]+"</a></li>";
                                        }
                                    }
                                    html+="</ul></body></html>";
                                    response.writeHead(200,{"Content-type":"text/html"});
                                    response.write(html);
                                    response.end();
                                }
                            });
                        }
                    }
               });
               
            }else{
                response.writeHead(404,{"Content-type":"text/plain"});
                response.write("The request url "+pathname+" not found!");
                response.end(); 
            }
        });
       
    });
    httpServer.listen(port,host);
    return httpServer;
}

function getRealPath(pathname,webRoot,routeMaps){
	var realPath=routeMaps[pathname];
	//查找路径映射
	if(!realPath){
		var paths=pathname.split("/"),dirname,dirMapName;
		for(var i=1;i<paths.length;i++){
			dirname=paths.slice(0,i).join("/");
			dirMapName=routeMaps[dirname];
			if(dirMapName){
				realPath=pathname.replace(dirname,dirMapName);
				break;
			}
		}
	}	
	realPath=realPath||path.join(webRoot,pathname);
	return realPath;
}

function listDir() {

}