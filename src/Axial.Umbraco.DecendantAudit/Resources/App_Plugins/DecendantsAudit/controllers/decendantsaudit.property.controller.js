function decendantsAuditPropertyController($scope, $routeParams, $location, $log, $timeout, editorState, decendantsAuditResource) {

    decendantsAuditResource.getAudit(editorState.current.key, $scope.model.config.decendantContentTypes).then(function (response) {
        if (response && response.data) {

            $scope.records = response.data;
            
            $timeout(function () {
                $('.decendants-audit-table').treetable({ expandable: true, initialState: 'expanded' });
            }, 0);
        }
    });

};
angular.module('umbraco').controller('DecendantsAudit.Property.Controller', decendantsAuditPropertyController);