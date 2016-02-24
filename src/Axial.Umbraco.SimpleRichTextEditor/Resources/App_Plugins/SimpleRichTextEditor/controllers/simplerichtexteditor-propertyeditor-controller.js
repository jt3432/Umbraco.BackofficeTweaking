
angular.module('umbraco').controller('Umbraco.SimpleRichTextEditorController', ['$scope', '$log', '$timeout', function ($scope, $log, $timeout) {  

    $scope.clean = function () {        
        var regex = /(<([^>]+)>)/ig;
        $scope.model.value = $('#' + $scope.model.alias).val().replace(regex, "");
    };
    $scope.bold = function () {
        $scope.model.value = $('#' + $scope.model.alias).wrapText('<strong>', '</strong>');
    };
    $scope.italic = function () {
        $scope.model.value = $('#' + $scope.model.alias).wrapText('<em>', '</em>');
    };
    $scope.strike = function () {
        $scope.model.value = $('#' + $scope.model.alias).wrapText('<strike>', '</strike>');
    };

    $.fn.wrapText = function (openTag, closeTag) {
        var $textField = this;
        var len = $textField.val().length;
        var start = $textField[0].selectionStart;
        var end = $textField[0].selectionEnd;
        var selectedText = $textField.val().substring(start, end);
        var replacement = openTag + selectedText + closeTag;
        return $textField.val().substring(0, start) + replacement + $textField.val().substring(end, len);
    };

}]);