/**
* Обьект через который мы и будем работать с текстовыми полями.
* Содержит все необходимые поля, легко расширяем по вкусу =)
* @argument  obj  HTMLElment  - текстовое поле, с кторым предстоит работать
* 
* Author: Sardar <Sardar@vingrad.ru>
*/
function TextAreaSelectionHelper(obj) {
  this.target=obj;
  this.target.carretHandler=this; //ссылка самого на себя для текстового поля
  /**
  * Помним, что события могут быть уже опредлеенны, тогда нужно использовать
  * очереди. Конечно, реализация для ИЕ и Мозиллы в корне различаются, почитать
  * и достать готовый код можно здесь: http://forum.vingrad.ru/index.php?showtopic=32350
  */
  this.target.onchange=_textareaSaver;
  this.target.onclick=_textareaSaver;
  this.target.onkeyup=_textareaSaver;
  this.target.onfocus=_textareaSaver;
  if(!document.selection) this.target.onSelect=_textareaSaver; //для Мозиллы
  
  this.start=-1;
  this.end=-1;
  this.scroll=-1;
  this.iesel=null; //для ИЕ
}
/**
* Достать отселектированный текст
*/
TextAreaSelectionHelper.prototype.getSelectedText=function() {
   return this.iesel? this.iesel.text: (this.start>=0&&this.end>this.start)? this.target.value.substring(this.start,this.end): "";
}
/**
* Вставить код под курсором. Если текст не отселектирован(не фокуса) и 
* позиция не взята, то вставить в конец текстового поля.
*
* @argument text String - заменить селекцию на этот текст
* @argument secondtag String - если задан, то селекция не заменяется, а обрамляется этими тегами
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
* Это функции для веса =)
*/
TextAreaSelectionHelper.prototype.getText=function() {
  return this.target.value;
}
TextAreaSelectionHelper.prototype.setText=function(text) {
  this.target.value=text;
}
/**
* Приватная фукнкция, записывающая позицию курсора
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
