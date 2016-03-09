angular.module('umbraco').controller('Umbraco.DeleteChildrenConfirmationController', ['$scope', '$log', '$timeout', 'navigationService', 'treeService', 'deleteChildrenResources', function ($scope, $log, $timeout, navigationService, treeService, deleteChildrenResources) {

    $scope.showMessage = false;
    $scope.message = '';

    $scope.performDelete = function () {
        //$log.info('Deleting children!');
        deleteChildrenResources.deleteChildren($scope.currentNode.id).then(function (response) {
            if (response.data) {
                treeService.removeChildNodes($scope.currentNode);
                navigationService.hideDialog();
            }
            else {
                $scope.message = 'The was a problem deleting child nodes. Please review the Umbraco logs for more information.';
                $scope.showMessage = true;
                $log.error($scope.message);
            }
        });
    };

    $scope.cancel = function () {
        navigationService.hideDialog();
    };

}]);