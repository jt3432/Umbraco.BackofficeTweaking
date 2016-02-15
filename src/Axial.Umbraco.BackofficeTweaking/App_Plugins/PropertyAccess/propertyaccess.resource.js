angular.module('umbraco.resources').factory('PropertyAccessResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {

        getRules: function () {
            return $http.get("backoffice/PropertyAccess/PropertyAccessApi/GetRules", {
                params: {}
            });
        },

        saveRules: function (configContent) {
            var data = JSON.stringify(angular.toJson(configContent));
            return $http.post("backoffice/PropertyAccess/PropertyAccessApi/SaveRules", data);
        }

    };
})

