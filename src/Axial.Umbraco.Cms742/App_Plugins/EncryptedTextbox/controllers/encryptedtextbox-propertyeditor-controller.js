angular.module('umbraco').controller('Umbraco.EncryptedTextboxController', ['$scope', '$log', '$timeout', 'editorState', 'encryptTextboxResources', function ($scope, $log, $timeout, editorState, encryptTextboxResources) {

    $scope.mode = 'password';

    encryptTextboxResources.decrypt($scope.model.value, editorState.current.key).then(function (response) {
        if (response.data) {
            $scope.model.value = JSON.parse(response.data);
        }        
    });

    $scope.show = function () {
        $timeout(function () {
            $scope.mode = 'text';
        }, 0);
    }

    $scope.hide = function () {
        $timeout(function () {
            $scope.mode = 'password';
        }, 0);
    }

}]);