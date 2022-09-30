
Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlModalProcess

        Inherits UserControl

        Private _disbale_form As Boolean = True

        Private _prg_fill_style As String = "fill1"
        Private _prg_image_blank As String = "blank.gif"
        Private _prg_max_value As Single = 1
        Private _prg_width As Integer = 200
        Private _prg_height As Integer = 10
        Private _prg_padding As Integer = 1


        Public Property DoDisableForm() As Boolean
            Get
                Return _disbale_form
            End Get
            Set(ByVal value As Boolean)
                _disbale_form = value
            End Set
        End Property

        Public Property ProgressFillStyle() As String
            Get
                Return _prg_fill_style
            End Get
            Set(ByVal value As String)
                _prg_fill_style = value
            End Set
        End Property

        Public Property ProgressBlankImage() As String
            Get
                Return _prg_image_blank
            End Get
            Set(ByVal value As String)
                _prg_image_blank = value
            End Set
        End Property

        Public Property ProgressWidth() As Integer
            Get
                Return _prg_width
            End Get
            Set(ByVal value As Integer)
                _prg_width = value
            End Set
        End Property

        Public Property ProgressHeight() As Integer
            Get
                Return _prg_height
            End Get
            Set(ByVal value As Integer)
                _prg_height = value
            End Set
        End Property

        Public Property ProgressPadding() As Integer
            Get
                Return _prg_padding
            End Get
            Set(ByVal value As Integer)
                _prg_padding = value
            End Set
        End Property

        Public Property ProgressMaxValue() As Single
            Get
                Return _prg_max_value
            End Get
            Set(ByVal value As Single)
                _prg_max_value = value
            End Set
        End Property

        Public Sub New()
            'If Page Is Nothing Then AddHandler Load, AddressOf InitComponent Else AddHandler Page.Load, AddressOf InitComponent
        End Sub

    End Class

End Namespace


'function disableElements(elements) {
'    for (var i = elements.length - 1; i >= 0; i--) {
'        var elmt = elements[i];
'        if (!elmt.disabled) {
'            elmt.disabled = true;
'        }
'        else {
'            elmt._wasDisabled = true;
'        }
'    }
'}

'function disableFormElements() {
'    disableElements(_form.getElementsByTagName("INPUT"));
'    disableElements(_form.getElementsByTagName("SELECT"));
'    disableElements(_form.getElementsByTagName("TEXTAREA"));
'    disableElements(_form.getElementsByTagName("BUTTON"));
'    disableElements(_form.getElementsByTagName("A"));
'}


'function enableElements(elements) {
'    for (var i = elements.length - 1; i >= 0; i--) {
'        var elmt = elements[i];
'        if (!elmt._wasDisabled) {
'            elmt.disabled = false;
'        }
'        else {
'            elmt._wasDisabled = null;
'        }
'    }
'}

'function enableFormElements() {
'    enableElements(_form.getElementsByTagName("INPUT"));
'    enableElements(_form.getElementsByTagName("SELECT"));
'    enableElements(_form.getElementsByTagName("TEXTAREA"));
'    enableElements(_form.getElementsByTagName("BUTTON"));
'    enableElements(_form.getElementsByTagName("A"));
'}
