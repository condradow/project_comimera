#!/bin/sh
java -Dfile.encoding=UTF-8 -Dlogback.configurationFile=logback.xml -jar server.jar "$@"