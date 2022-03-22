<?php
$archive = simplexml_load_file("GalnetArchive.xml");
echo 
'{
  "GalnetArchive":{
    "version":'.$archive->version.',
    
