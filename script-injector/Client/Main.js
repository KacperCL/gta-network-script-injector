/// <reference path="../../../types-gtanetwork/index.d.ts" />
API.onServerEventTrigger.connect((eventName, arguments) => {
    if (eventName != "INJECT_SCRIPT")
        return;
    var code = arguments[0];
    eval(code);
});
