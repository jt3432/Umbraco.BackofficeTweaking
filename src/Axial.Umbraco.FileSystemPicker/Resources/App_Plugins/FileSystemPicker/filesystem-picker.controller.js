
function folderSystemPickerController($scope, dialogService) {
    $scope.openPicker = function () {
        dialogService.open({
            template: '/App_Plugins/FileSystemPicker/foldersystem-picker-dialog.html',
            callback: populate
        });
    };
    function populate(data) {
        $scope.model.value = '/' + data == '-1' ? '' : data;
    };

};
angular.module('umbraco').controller('Umbraco.FolderSystemPickerController', folderSystemPickerController);


function folderSystemPickerDialogController($scope, dialogService) {
    $scope.dialogEventHandler = $({});
    $scope.dialogEventHandler.bind('treeNodeSelect', nodeSelectHandler);

    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();
        $scope.submit(args.node.id);
    };
};
angular.module('umbraco').controller('Umbraco.FolderSystemPickerDialogController', folderSystemPickerDialogController);


function fileSystemPickerController($scope, $http, $routeParams, dialogService) {

    $scope.openPicker = function () {

        var startFolder = $scope.model.config.folder;

        if ($scope.model.config.startFolderNamePropertyAlias) {
            var alias = $scope.model.config.startFolderNamePropertyAlias;
            var removeChars = $scope.model.config.removeCharactersPropertyAlias;
            var id = $routeParams.id;
            $http.get('/umbraco/backoffice/FileSystemPicker/FileSystemPickerApi/GetStartFolderName/?startFolderNamePropertyAlias=' + escape(alias) + '&removeCharactersPropertyAlias=' + escape(removeChars) + '&currentNodeId=' + id)
                .then(function (response) {
                    if (response && response.data && response.data.folderName != null) {
                        startFolder = $scope.model.config.folder + '/' + response.data.folderName;
                    }

                    fileSystemPickerTreeDialog = dialogService.open({
                        template: '/App_Plugins/FileSystemPicker/filesystem-picker-dialog.html',
                        callback: populate,
                        dialogData: {
                            filter: $scope.model.config.filter,
                            folder: startFolder,
                            managementMode: $scope.model.config.managementMode
                        }
                    });

                }, function (data) {
                    $log.error(data)
                });
        }
    };

    //a method to update the model by adding a blank item
    $scope.clickMap = function ($event) {
        var mapImage = $('#usda-map-image')

        var offset_t = mapImage.offset().top - $(window).scrollTop();
        var offset_l = mapImage.offset().left - $(window).scrollLeft();

        var left = Math.round(($event.clientX - offset_l));
        var top = Math.round(($event.clientY - offset_t));

        var $emptyInput = $('input[id*=mapCoordinates]').filter(function () { return !this.value; });

        $emptyInput.val(left + ',' + top);
   } 

    $scope.remove = function () {
        $scope.model.value = '';
    };

    function populate(data) {
        $scope.model.value = data;
    };
};
angular.module('umbraco').controller('Umbraco.FileSystemPickerController', fileSystemPickerController);


function fileSystemPickerDialogController($rootScope, $scope, $log, dialogService, treeService) {

    $scope.dialogEventHandler = $({});
    $scope.dialogEventHandler.bind('treeNodeSelect', nodeSelectHandler);
    $scope.dialogEventHandler.bind('treeOptionsClick', optionsHandler);

    function optionsHandler(node, ev) {
        $rootScope.fileSytemPickerOptionsMenuDialog = dialogService.open({
            template: '/App_Plugins/FileSystemPicker/filesystem-picker-options-menu.html',
            dialogData: {
                node: ev.node,
                folder: ev.node.id == '-1' ? ev.node.metaData.startfolder : ev.node.id
            }
        });
    };

    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();
        if (args.node.icon !== 'icon-folder' && !args.node.metaData.managementMode) {
            $scope.submit(args.node.id);
        }
    };

    $scope.dialogEventHandler.bind('treeNodeExpanded', nodeExpandedHandler);
    $scope.dialogEventHandler.bind('treeLoaded', treeLoadedHandler);

    function nodeExpandedHandler(ev, args) {
        var node = args.node;
        var children = args.children;
        if (node && node.id === '-1' && children && children.length === 1) {
            var rootFolder = children[0];
            treeService.syncTree({
                node: rootFolder,
                path: [node.id, rootFolder.id],
                forceReload: true
            })
        }
    };
    function treeLoadedHandler(ev, args) { };
};
angular.module('umbraco').controller('Umbraco.FileSystemPickerDialogController', fileSystemPickerDialogController);


function fileSystemPickerOptionsMenuController($rootScope, $scope, $timeout, $log, dialogService, treeService) {

    $scope.openUpload = function (node) {
        $rootScope.fileSytemPickerUploadDialog = dialogService.open({
            template: '/App_Plugins/FileSystemPicker/filesystem-picker-upload-dialog.html',
            closeCallback: dialogClosed,
            dialogData: {
                node: node
            }
        });
        $timeout(function () {
            dialogService.close($rootScope.fileSytemPickerOptionsMenuDialog, null);
        }, 500);
    };

    $scope.openCreate = function (node) {
        $rootScope.fileSytemPickerNewFolderDialog = dialogService.open({
            template: '/App_Plugins/FileSystemPicker/filesystem-picker-newfolder-dialog.html',
            closeCallback: dialogClosed,
            dialogData: {
                node: node
            }
        });
        $timeout(function () {
            dialogService.close($rootScope.fileSytemPickerOptionsMenuDialog, null);
        }, 500);
    };

    $scope.openRename = function (node) {
        $rootScope.fileSytemPickerRenameDialog = dialogService.open({
            template: '/App_Plugins/FileSystemPicker/filesystem-picker-rename-dialog.html',
            closeCallback: dialogClosedParentRefresh,
            dialogData: {
                node: node
            }
        });
        $timeout(function () {
            dialogService.close($rootScope.fileSytemPickerOptionsMenuDialog, null);
        }, 500);
    };

    $scope.openDelete = function (node) {
        $rootScope.fileSytemPickerDeleteDialog = dialogService.open({
            template: '/App_Plugins/FileSystemPicker/filesystem-picker-delete-dialog.html',
            closeCallback: dialogClosedParentRefresh,
            dialogData: {
                node: node
            }
        });
        $timeout(function () {
            dialogService.close($rootScope.fileSytemPickerOptionsMenuDialog, null);
        }, 500);
    };

    function dialogClosedParentRefresh(node) {
        //$log.debug('Upload Closed: ', data.node);
        if (node) {
            if (node.id === -1) {

            } else {
                try {
                    node.parent().refresh();                    
                } catch (e) {
                    $log.error('Error refreshing node: ', e);
                }
            }
        }
    };

    function dialogClosed(node) {
        //$log.debug('Upload Closed: ', data.node);
        if (node) {
            if (node.id === -1) {

            } else {
                try {
                    if (node.icon === 'icon-folder') {
                        node.refresh();
                    }
                    else if (node.icon === 'icon-document') {
                        node.parent().refresh();
                    }
                } catch (e) {
                    $log.error('Error refreshing node: ', e);
                }
            }
        }
    };
};
angular.module('umbraco').controller('Umbraco.FileSystemPickerOptionsMenuController', fileSystemPickerOptionsMenuController);


function fileSystemPickerUploadDialogController($rootScope, $scope, $log, dialogService) {

    $scope.onUploadComplete = function (data) {
        //$log.debug('Upload Complete: ', data);        
        try {
            dialogService.close($rootScope.fileSytemPickerUploadDialog, data.node);
        } catch (e) {
            $log.error('Error closing dialog : ', e);
        }
    };

    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();
        if (args.node.icon !== 'icon-folder') {
            $scope.submit(args.node.id);
        }
    };

};
angular.module('umbraco').controller('Umbraco.FileSystemPickerUploadDialogController', fileSystemPickerUploadDialogController);


function fileSystemPickerNewFolderDialogController($rootScope, $scope, $http, $log, dialogService) {

    $scope.folderName = ''

    $scope.createFolder = function (node) {
        if ($scope.folderName) {
            var path = node.id + '/' + $scope.folderName;
            $http.post('/umbraco/backoffice/FileSystemPicker/FileSystemPickerApi/PostCreateFolder/?folderPath=' + path, null)
                .then(function (data) {
                    dialogService.close($rootScope.fileSytemPickerNewFolderDialog, node);
                }, errorCallback);
        }
    };

    function errorCallback(e) {
        $log.error('Error creating folder: ', e);
    }

};
angular.module('umbraco').controller('Umbraco.FileSystemPickerNewFolderDialogController', fileSystemPickerNewFolderDialogController);


function fileSystemPickerRenameDialogController($rootScope, $scope, $http, $log, dialogService) {

    $scope.newName = ''

    $scope.rename = function (node) {
        if ($scope.newName) {
            $http.post('/umbraco/backoffice/FileSystemPicker/FileSystemPickerApi/PostRename/?path=' + node.id + '&name=' + $scope.newName, null)
                .then(function (data) {
                    dialogService.close($rootScope.fileSytemPickerRenameDialog, node);
                }, errorCallback);
        }
    };

    function errorCallback(e) {
        $log.error('Error renaming item: ', e);
    }

};
angular.module('umbraco').controller('Umbraco.FileSystemPickerRenameDialogController', fileSystemPickerRenameDialogController);


function fileSystemPickerDeleteDialogController($rootScope, $scope, $http, $log, dialogService) {

    $scope.delete = function (node) {
        $http.post('/umbraco/backoffice/FileSystemPicker/FileSystemPickerApi/PostDelete/?path=' + node.id, null)
            .then(function (data) {
                dialogService.close($rootScope.fileSytemPickerDeleteDialog, node);
            }, errorCallback);
    };

    function errorCallback(e) {
        $log.error('Error deleting item: ', e);
    }

};
angular.module('umbraco').controller('Umbraco.FileSystemPickerDeleteDialogController', fileSystemPickerDeleteDialogController);


