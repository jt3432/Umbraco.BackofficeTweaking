angular.module('umbraco.resources').factory('encryptTextboxResources', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {
        encrypt: function (value, key) {
            return $http.get('backoffice/EncryptedTextbox/EncryptedTextboxApi/Encrypt/?value=' + value + '&key=' + key);
        },
        decrypt: function (value, key) {
            return $http.get('backoffice/EncryptedTextbox/EncryptedTextboxApi/Decrypt/?value=' + value + '&key=' + key);
        }
    };
});