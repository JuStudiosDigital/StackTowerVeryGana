mergeInto(LibraryManager.library, {
  Copagos_PostMessageToParent: function (jsonMessagePtr, targetOriginPtr) {
    try {
      var jsonMessage = UTF8ToString(jsonMessagePtr);
      var targetOrigin = UTF8ToString(targetOriginPtr);

      if (!targetOrigin) {
        targetOrigin = "*";
      }

      var messagePayload;

      try {
        messagePayload = JSON.parse(jsonMessage);
      } catch (parseError) {
        console.warn(
          "[CopagosProductBridge] El mensaje no era un JSON válido. Se enviará como texto.",
          parseError
        );

        messagePayload = jsonMessage;
      }

      var targetWindow =
        window.parent && window.parent !== window
          ? window.parent
          : window;

      targetWindow.postMessage(messagePayload, targetOrigin);

      console.log(
        "[CopagosProductBridge] Mensaje enviado:",
        messagePayload
      );
    } catch (error) {
      console.error(
        "[CopagosProductBridge] Error enviando mensaje al frontend:",
        error
      );
    }
  }
});