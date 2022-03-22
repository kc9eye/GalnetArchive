<?php
$archive = simplexml_load_file("GalnetArchive.xml");
echo 
'{
  "GalnetArchive":{
    "version":'.$archive->version.',
    "articles":[
    '
foreach($archive->article as $article) {
  echo '{"id":"'.$article->id.'","link":"'.$article->link.'","title":"'.$article->title.'","date":"'.$article->date.'","story":"'.htmlspecialchars($article->story,ENT_QUOTES,'UTF-8').'"},';
}
echo ']}';
