/**
* ������ ����� ������� �� � ����� �������� � ���������� ������.
* �������� ��� ����������� ����, ����� ��������� �� ����� =)
* @argument  obj  HTMLElment  - ��������� ����, � ������ ��������� ��������
* 
* Author: Sardar <Sardar@vingrad.ru>
*/
function TextAreaSelectionHelper(obj) {
  this.target=obj;
  this.target.carretHandler=this; //������ ������ �� ���� ��� ���������� ����
  /**
  * ������, ��� ������� ����� ���� ��� �����������, ����� ����� ������������
  * �������. �������, ���������� ��� �� � ������� � ����� �����������, ��������
  * � ������� ������� ��� ����� �����: http://forum.vingrad.ru/index.php?showtopic=32350
  */
  this.target.onchange=_textareaSaver;
  this.target.onclick=_textareaSaver;
  this.target.onkeyup=_textareaSaver;
  this.target.onfocus=_textareaSaver;
  if(!document.selection) this.target.onSelect=_textareaSaver; //��� �������
  
  this.start=-1;
  this.end=-1;
  this.scroll=-1;
  this.iesel=null; //��� ��
}
/**
* ������� ����������������� �����
*/
TextAreaSelectionHelper.prototype.getSelectedText=function() {
   return this.iesel? this.iesel.text: (this.start>=0&&this.end>this.start)? this.target.value.substring(this.start,this.end): "";
}
/**
* �������� ��� ��� ��������. ���� ����� �� ��������������(�� ������) � 
* ������� �� �����, �� �������� � ����� ���������� ����.
*
* @argument text String - �������� �������� �� ���� �����
* @argument secondtag String - ���� �����, �� �������� �� ����������, � ����������� ����� ������
*/
TextAreaSelectionHelper.prototype.setSelectedText=function(text, secondtag) {
  if(this.iesel) {
    if(typeof(secondtag)=="string") {
   var l=this.iesel.text.length;
      this.iesel.text=text+this.iesel.text+secondtag;
   this.iesel.moveEnd("character", -secondtag.length);
    this.iesel.moveStart("character", -l);   
    } else {
   this.iesel.text=text;
    }
    this.iesel.select();
  } else if(this.start>=0&&this.end>=this.start) {
     var left=this.target.value.substring(0,this.start);
     var right=this.target.value.substr(this.end);
  var scont=this.target.value.substring(this.start, this.end);
  if(typeof(secondtag)=="string") {
    this.target.value=left+text+scont+secondtag+right;
    this.end=this.target.selectionEnd=this.start+text.length+scont.length;
    this.start=this.target.selectionStart=this.start+text.length;    
  } else {
       this.target.value=left+text+right;
    this.end=this.target.selectionEnd=this.start+text.length;
    this.start=this.target.selectionStart=this.start+text.length;
  }
  this.target.scrollTop=this.scroll;
  this.target.focus();
  } else {
    this.target.value+=text + ((typeof(secondtag)=="string")? secondtag: "");
    if(this.scroll>=0) this.target.scrollTop=this.scroll;
  }
}
/**
* ��� ������� ��� ���� =)
*/
TextAreaSelectionHelper.prototype.getText=function() {
  return this.target.value;
}
TextAreaSelectionHelper.prototype.setText=function(text) {
  this.target.value=text;
}
/**
* ��������� ��������, ������������ ������� �������
*/
function _textareaSaver() {
  if(document.selection) {
    this.carretHandler.iesel = document.selection.createRange().duplicate();
  } else if(typeof(this.selectionStart)!="undefined") {
    this.carretHandler.start=this.selectionStart;
    this.carretHandler.end=this.selectionEnd;
    this.carretHandler.scroll=this.scrollTop;
  } else {this.carretHandler.start=this.carretHandler.end=-1;}
}
