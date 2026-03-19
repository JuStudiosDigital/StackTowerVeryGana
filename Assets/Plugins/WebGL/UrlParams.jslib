mergeInto(LibraryManager.library, {
  GetURLParameter: function (paramPtr) {
    var paramName = UTF8ToString(paramPtr);

    try {
      var urlParams = new URLSearchParams(window.location.search);
      var value = urlParams.get(paramName);

      if (value === null)
        value = "";

      var bufferSize = lengthBytesUTF8(value) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(value, buffer, bufferSize);
      return buffer;
    }
    catch (e) {
      var empty = "";
      var bufferSize = lengthBytesUTF8(empty) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(empty, buffer, bufferSize);
      return buffer;
    }
  }
});
