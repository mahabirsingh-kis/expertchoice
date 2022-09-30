CKEDITOR.plugins.add('cancel',
    {
        init: function (editor) {
            var pluginName = 'cancel';
            editor.ui.addButton('cancel',
                {
                    label: 'Cancel',
                    command: 'OpenWindow',
                    icon: this.path + 'cancel.png'
                });
            // var cmd = editor.addCommand('OpenWindow', { exec: showMyDialog });
        }
    });