angular.module('umbraco.resources').factory('decendantsAuditResource', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {

        getAudit: function (id, aliases) {
            return $http.get("backoffice/DecendantsAudit/DecendantsAuditApi/GetAudit?id=" + id + '&aliases=' + aliases, {
                params: {}
            });
        }
    };
})

