angular.module('umbraco.resources').factory('deleteChildrenResources', function ($q, $http, $log, umbRequestHelper, angularHelper) {
    return {
        deleteChildren: function (parentId, userId) {
            return $http.delete("backoffice/DeleteChildren/DeleteChildrenApi/Delete?parentId=" + parentId);
        }
    };
});