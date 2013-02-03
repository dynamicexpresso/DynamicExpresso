// Dynamic Expresso Web Shell javascript code is 
// based on the mongulator project (https://github.com/banker/mongulator)
//
// Copyright (c) 2009 Kyle Banker
// Licensed under the MIT Licence.
// http://www.opensource.org/licenses/mit-license.php


var DefaultInputHtml = function (stack) {
    var linePrompt = "";
    if (stack == 0) {
        linePrompt += "<span class='prompt'> ></span>";
    }
    else {
        for (var i = 0; i <= stack; i++) {
            linePrompt += "<span class='prompt'>.</span>";
        }
    }
    return "<div class='line'>" +
           linePrompt +
           "<input type='text' class='readLine active' />" +
           "</div>";
}

var EnterKeyCode = 13;
var UpArrowKeyCode = 38;
var DownArrowKeyCode = 40;

var PTAG = function (str) {
    return "<pre>" + str + "</pre>";
}

var BR = function () {
    return "<br/>";
}

// Readline class to handle line input.
var ReadLine = function (options) {
    this.options = options || {};
    this.htmlForInput = this.options.htmlForInput;
    this.inputHandler = this.options.handler;
    //this.scoper = this.options.scoper;
    //  this.connection   = new Connection();
    this.terminal = $(this.options.terminalId || "#terminal");
    this.lineClass = this.options.lineClass || '.readLine';
    this.history = [];
    this.historyPtr = 0;

    this.initialize();
};

ReadLine.prototype = {

    initialize: function () {
        this.addInputLine();

        var inputElement = this.lineClass + '.active';
        var $terminal = this.terminal;
        this.terminal.click(function (event) {
            var $target = $(event.target);
            if ($target.is($terminal))
                $(inputElement).focus();
        });
    },

    // Enter a new input line with proper behavior.
    addInputLine: function (stackLevel) {
        stackLevel = stackLevel || 0;
        this.terminal.append(this.htmlForInput(stackLevel));
        var ctx = this;
        ctx.activeLine = $(this.lineClass + '.active');

        // Bind key events for entering and navigting history.
        ctx.activeLine.bind("keydown", function (ev) {
            switch (ev.keyCode) {
                case EnterKeyCode:
                    ctx.processInput(this.value);
                    break;
                case UpArrowKeyCode:
                    ctx.getCommand('previous');
                    break;
                case DownArrowKeyCode:
                    ctx.getCommand('next');
                    break;
            }
        });

        this.activeLine.focus();
    },

    // Returns the 'next' or 'previous' command in this history.
    getCommand: function (direction) {
        if (this.history.length === 0) {
            return;
        }
        this.adjustHistoryPointer(direction);
        this.activeLine[0].value = this.history[this.historyPtr];
        $(this.activeLine[0]).focus();
        //this.activeLine[0].value = this.activeLine[0].value;
    },

    // Moves the history pointer to the 'next' or 'previous' position. 
    adjustHistoryPointer: function (direction) {
        if (direction == 'previous') {
            if (this.historyPtr - 1 >= 0) {
                this.historyPtr -= 1;
            }
        }
        else {
            if (this.historyPtr + 1 < this.history.length) {
                this.historyPtr += 1;
            }
        }
    },

    // Return the handler's response.
    processInput: function (expression) {
        var me = this;
        this.inputHandler.eval(expression, function (response) {
            me.processOutput(expression, response);
                });
    },

    processOutput: function (expression, response) {
        this.insertResponse(response.result);

        // Save to the command history...
        if ((lineValue = expression.trim()) !== "") {
            this.history.push(lineValue);
            this.historyPtr = this.history.length;
        }

        // deactivate the line...
        this.activeLine.value = "";
        this.activeLine.attr({ disabled: true });
        this.activeLine.removeClass('active');

        // and add a new command line.
        this.addInputLine(response.stack);
    },

    insertResponse: function (response) {
        var pClass = "response";
        if (!response.success)
            pClass += " error";

        var $newP = $("<pre class='" + pClass + "'></pre>")
        $newP.text(response.result);

        this.activeLine.parent().append($newP);
    }
};


$htmlFormat = function (obj) {
    return tojson(obj, ' ', ' ', true);
}



var DynamicExpressoHandler = function (options) {
    this.options = options || {};
    this._commandStack = 0;
    this._interpreterUrl = this.options.interpreterUrl;
};

DynamicExpressoHandler.prototype = {

    eval: function (inputString, onSuccedeed) {

        $.ajax({
            type: "POST",
            url: this._interpreterUrl,
            data: { expression: inputString },
            dataType: "json"
        }).done(function (result) {
            onSuccedeed({ stack: this._commandStack, result: result });
        });
    }
};

