rmdir Application\App_Data\MailMerge\tmp /s /q
sed\7z x Application\App_Data\MailMerge\%1 -oApplication\App_Data\MailMerge\Tmp
sed\sed -i "s#file:*///[^\"]*ComparionInvitations#ComparionInvitations#g" Application\App_Data\MailMerge\tmp\word\_rels\settings.xml.rels
sed\sed -i "s#Source=[^\"]*ComparionInvitations#Source=ComparionInvitations#g" Application\App_Data\MailMerge\tmp\word\settings.xml
cd Application\App_Data\MailMerge\tmp
..\..\..\..\sed\7z a MM.zip *
cd ..\..\..\..
copy Application\App_Data\MailMerge\tmp\MM.zip Application\App_Data\MailMerge\%1
rmdir Application\App_Data\MailMerge\tmp /s /q
