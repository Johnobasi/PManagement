/*	CSTruter Listpicker Control JS File version 1.0
Author: Christoff Truter

Date Created: 12 June 2008
	
e-Mail: christoff@cstruter.com
Website: www.cstruter.com
Copyright 2006-2008 CSTruter*/

function move(fromID, toID, containerID) {
    var from = document.getElementById(fromID);
    var to = document.getElementById(toID);

    for (var i = 0; i < from.options.length; i++) {
        if (from.options[i].selected) {
            to.options.add(new Option(from.options[i].text, from.options[i].value))
            from.remove(i--);
        }
    }

    var container = document.getElementById(containerID);
    container.value = escape("<listboxes>" + serialize(from) + serialize(to) + "</listboxes>");
}

function serialize(dropdown) {
    var value = '<' + dropdown.id + '>';
    for (var i = 0; i < dropdown.options.length; i++) {
        value += '<option><key><![CDATA[' + encodeURIComponent(dropdown.options[i].text) + ']]></key><value><![CDATA[' + encodeURIComponent(dropdown.options[i].value) + ']]></value></option>';
    }
    value += '</' + dropdown.id + '>';
    return value
}

function unselect(listbox) {
    document.getElementById(listbox).selectedIndex = -1;
}

function InputToUpper(inputText)
{
    var v = inputText.value;
    var v2 = v.toUpperCase();
    inputText.value = v2;
}