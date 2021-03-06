﻿/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTree
* @restrict E
**/

angular.module('umbraco.directives')
.directive('fsTree', function ($compile, $log, $q, $rootScope, treeService, notificationsService, $timeout, userService) {

    return {
        restrict: 'E',
        replace: true,
        terminal: false,

        scope: {
            section: '@',
            treealias: '@',
            hideoptions: '@',
            hideheader: '@',
            cachekey: '@',
            isdialog: '@',
            title: '@',
            //Custom query string arguments to pass in to the tree as a string, example: 'startnodeid=123&something=value'
            customtreeparams: '@',
            eventhandler: '=',
            enablecheckboxes: '@',
            enablelistviewsearch: '@'
        },

        compile: function (element, attrs) {
            //config
            //var showheader = (attrs.showheader !== 'false');
            var hideoptions = (attrs.hideoptions === 'true') ? 'hide-options' : '';
            var template = '<ul class="umb-tree ' + hideoptions + '"><li class="root">';
            template += '<div ng-hide="hideheader" on-right-click="altSelect(tree.root, $event)">' +
                '<h5>' +
                '<i ng-if="enablecheckboxes == \'true\'" ng-class="selectEnabledNodeClass(tree.root)"></i>' +
                '<a href="#/{{section}}" ng-click="select(tree.root, $event)" class="root-link">{{tree.name}}</a></h5>' +
                //'<a href="#/{{section}}" ng-click="select(tree.root, $event)" class="root-link">{{title ? title : tree.name}}</a></h5>' +
                //'<a class="umb-options" ng-hide="tree.root.isContainer || !tree.root.menuUrl" ng-click="options(tree.root, $event)" ng-swipe-right="options(tree.root, $event)"><i></i><i></i><i></i></a>' +
                '</div>';
            template += '<ul>' +
                '<fs-tree-item ng-repeat="child in tree.root.children" eventhandler="eventhandler" node="child" current-node="currentNode" tree="this" section="{{section}}" ng-animate="animation()"></fs-tree-item>' +
                '</ul>' +
                '</li>' +
                '</ul>';

            element.replaceWith(template);

            return function (scope, elem, attr, controller) {

                //flag to track the last loaded section when the tree 'un-loads'. We use this to determine if we should
                // re-load the tree again. For example, if we hover over 'content' the content tree is shown. Then we hover
                // outside of the tree and the tree 'un-loads'. When we re-hover over 'content', we don't want to re-load the 
                // entire tree again since we already still have it in memory. Of course if the section is different we will
                // reload it. This saves a lot on processing if someone is navigating in and out of the same section many times
                // since it saves on data retreival and DOM processing.
                var lastSection = '';

                //setup a default internal handler
                if (!scope.eventhandler) {
                    scope.eventhandler = $({});
                }

                //flag to enable/disable delete animations
                var deleteAnimations = false;


                /** Helper function to emit tree events */
                function emitEvent(eventName, args) {
                    if (scope.eventhandler) {
                        $(scope.eventhandler).trigger(eventName, args);
                    }
                }

                /** This will deleteAnimations to true after the current digest */
                function enableDeleteAnimations() {
                    //do timeout so that it re-enables them after this digest
                    $timeout(function () {
                        //enable delete animations
                        deleteAnimations = true;
                    }, 0, false);
                }


                /*this is the only external interface a tree has */
                function setupExternalEvents() {
                    if (scope.eventhandler) {

                        scope.eventhandler.clearCache = function (section) {
                            treeService.clearCache({ section: section });
                        };

                        scope.eventhandler.load = function (section) {
                            scope.section = section;
                            loadTree();
                        };

                        scope.eventhandler.reloadNode = function (node) {

                            if (!node) {
                                node = scope.currentNode;
                            }

                            if (node) {
                                scope.loadChildren(node, true);
                            }
                        };

                        /** 
                            Used to do the tree syncing. If the args.tree is not specified we are assuming it has been 
                            specified previously using the _setActiveTreeType
                        */
                        scope.eventhandler.syncTree = function (args) {
                            if (!args) {
                                throw 'args cannot be null';
                            }
                            if (!args.path) {
                                throw 'args.path cannot be null';
                            }

                            var deferred = $q.defer();

                            //this is super complex but seems to be working in other places, here we're listening for our
                            // own events, once the tree is sycned we'll resolve our promise.
                            scope.eventhandler.one('treeSynced', function (e, syncArgs) {
                                deferred.resolve(syncArgs);
                            });

                            //this should normally be set unless it is being called from legacy 
                            // code, so set the active tree type before proceeding.
                            if (args.tree) {
                                loadActiveTree(args.tree);
                            }

                            if (angular.isString(args.path)) {
                                args.path = args.path.replace('"', '').split(',');
                            }

                            //reset current node selection
                            //scope.currentNode = null;

                            //Filter the path for root node ids (we don't want to pass in -1 or 'init')

                            args.path = _.filter(args.path, function (item) { return (item !== 'init' && item !== '-1'); });

                            //Once those are filtered we need to check if the current user has a special start node id, 
                            // if they do, then we're going to trim the start of the array for anything found from that start node
                            // and previous so that the tree syncs properly. The tree syncs from the top down and if there are parts
                            // of the tree's path in there that don't actually exist in the dom/model then syncing will not work.

                            userService.getCurrentUser().then(function (userData) {

                                var startNodes = [userData.startContentId, userData.startMediaId];
                                _.each(startNodes, function (i) {
                                    var found = _.find(args.path, function (p) {
                                        return String(p) === String(i);
                                    });
                                    if (found) {
                                        args.path = args.path.splice(_.indexOf(args.path, found));
                                    }
                                });


                                loadPath(args.path, args.forceReload, args.activate);

                            });

                            return deferred.promise;
                        };

                        /** 
                            Internal method that should ONLY be used by the legacy API wrapper, the legacy API used to 
                            have to set an active tree and then sync, the new API does this in one method by using syncTree.
                            loadChildren is optional but if it is set, it will set the current active tree and load the root
                            node's children - this is synonymous with the legacy refreshTree method - again should not be used
                            and should only be used for the legacy code to work.
                        */
                        scope.eventhandler._setActiveTreeType = function (treeAlias, loadChildren) {
                            loadActiveTree(treeAlias, loadChildren);
                        };
                    }
                }


                //helper to load a specific path on the active tree as soon as its ready
                function loadPath(path, forceReload, activate) {

                    if (scope.activeTree) {
                        syncTree(scope.activeTree, path, forceReload, activate);
                    }
                    else {
                        scope.eventhandler.one('activeTreeLoaded', function (e, args) {
                            syncTree(args.tree, path, forceReload, activate);
                        });
                    }
                }


                //given a tree alias, this will search the current section tree for the specified tree alias and
                //set that to the activeTree
                //NOTE: loadChildren is ONLY used for legacy purposes, do not use this when syncing the tree as it will cause problems
                // since there will be double request and event handling operations.
                function loadActiveTree(treeAlias, loadChildren) {
                    scope.activeTree = undefined;

                    function doLoad(tree) {
                        var childrenAndSelf = [tree].concat(tree.children);
                        scope.activeTree = _.find(childrenAndSelf, function (node) {
                            if (node && node.metaData && node.metaData.treeAlias) {
                                return node.metaData.treeAlias.toUpperCase() === treeAlias.toUpperCase();
                            }
                            return false;
                        });

                        if (!scope.activeTree) {
                            throw 'Could not find the tree ' + treeAlias + ', activeTree has not been set';
                        }

                        //This is only used for the legacy tree method refreshTree!
                        if (loadChildren) {
                            scope.activeTree.expanded = true;
                            scope.loadChildren(scope.activeTree, false).then(function () {
                                emitEvent('activeTreeLoaded', { tree: scope.activeTree });
                            });
                        }
                        else {
                            emitEvent('activeTreeLoaded', { tree: scope.activeTree });
                        }
                    }

                    if (scope.tree) {
                        doLoad(scope.tree.root);
                    }
                    else {
                        scope.eventhandler.one('treeLoaded', function (e, args) {
                            doLoad(args.tree.root);
                        });
                    }
                }


                /** Method to load in the tree data */

                function loadTree() {
                    if (!scope.loading && scope.section) {
                        scope.loading = true;

                        //anytime we want to load the tree we need to disable the delete animations
                        deleteAnimations = false;

                        //default args
                        var args = { section: scope.section, tree: scope.treealias, cacheKey: scope.cachekey, isDialog: scope.isdialog ? scope.isdialog : false };

                        //add the extra query string params if specified
                        if (scope.customtreeparams) {
                            args['queryString'] = scope.customtreeparams;
                        }

                        treeService.getTree(args)
                            .then(function (data) {
                                //set the data once we have it
                                scope.tree = data;

                                enableDeleteAnimations();

                                scope.loading = false;

                                //set the root as the current active tree
                                scope.activeTree = scope.tree.root;
                                emitEvent('treeLoaded', { tree: scope.tree });
                                emitEvent('treeNodeExpanded', { tree: scope.tree, node: scope.tree.root, children: scope.tree.root.children });
                                if (scope.tree.root.children.length === 1) {

                                }

                            }, function (reason) {
                                scope.loading = false;
                                notificationsService.error('Tree Error', reason);
                            });
                    }
                }

                /** syncs the tree, the treeNode can be ANY tree node in the tree that requires syncing */
                function syncTree(treeNode, path, forceReload, activate) {

                    deleteAnimations = false;

                    treeService.syncTree({
                        node: treeNode,
                        path: path,
                        forceReload: forceReload
                    }).then(function (data) {

                        if (activate === undefined || activate === true) {
                            scope.currentNode = data;
                        }

                        emitEvent('treeSynced', { node: data, activate: activate });

                        enableDeleteAnimations();
                    });

                }

                scope.selectEnabledNodeClass = function (node) {
                    return node ?
                        node.selected ?
                        'icon umb-tree-icon sprTree icon-check blue temporary' :
                        '' :
                        '';
                };

                /** method to set the current animation for the node. 
                 *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
                 *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
                 */
                scope.animation = function () {
                    if (deleteAnimations && scope.tree && scope.tree.root && scope.tree.root.expanded) {
                        return { leave: 'tree-node-delete-leave' };
                    }
                    else {
                        return {};
                    }
                };

                /* helper to force reloading children of a tree node */
                scope.loadChildren = function (node, forceReload) {
                    var deferred = $q.defer();

                    //emit treeNodeExpanding event, if a callback object is set on the tree
                    emitEvent('treeNodeExpanding', { tree: scope.tree, node: node });

                    //standardising                
                    if (!node.children) {
                        node.children = [];
                    }

                    if (forceReload || (node.hasChildren && node.children.length === 0)) {
                        //get the children from the tree service
                        treeService.loadNodeChildren({ node: node, section: scope.section })
                            .then(function (data) {
                                //emit expanded event
                                emitEvent('treeNodeExpanded', { tree: scope.tree, node: node, children: data });

                                enableDeleteAnimations();

                                deferred.resolve(data);
                            });
                    }
                    else {
                        emitEvent('treeNodeExpanded', { tree: scope.tree, node: node, children: node.children });
                        node.expanded = true;

                        enableDeleteAnimations();

                        deferred.resolve(node.children);
                    }

                    return deferred.promise;
                };

                /**
                  Method called when the options button next to the root node is called.
                  The tree doesnt know about this, so it raises an event to tell the parent controller
                  about it.
                */
                scope.options = function (n, ev) {
                    emitEvent('treeOptionsClick', { element: elem, node: n, event: ev });
                };

                /**
                  Method called when an item is clicked in the tree, this passes the 
                  DOM element, the tree node object and the original click
                  and emits it as a treeNodeSelect element if there is a callback object
                  defined on the tree
                */
                scope.select = function (n, ev) {
                    //on tree select we need to remove the current node - 
                    // whoever handles this will need to make sure the correct node is selected
                    //reset current node selection
                    scope.currentNode = null;

                    emitEvent('treeNodeSelect', { element: elem, node: n, event: ev });
                };

                scope.altSelect = function (n, ev) {
                    emitEvent('treeNodeAltSelect', { element: elem, tree: scope.tree, node: n, event: ev });
                };

                //watch for section changes
                scope.$watch('section', function (newVal, oldVal) {

                    if (!scope.tree) {
                        loadTree();
                    }

                    if (!newVal) {
                        //store the last section loaded
                        lastSection = oldVal;
                    }
                    else if (newVal !== oldVal && newVal !== lastSection) {
                        //only reload the tree data and Dom if the newval is different from the old one
                        // and if the last section loaded is different from the requested one.
                        loadTree();

                        //store the new section to be loaded as the last section
                        //clear any active trees to reset lookups
                        lastSection = newVal;
                    }
                });

                setupExternalEvents();
                loadTree();
            };
        }
    };
});



/**
 * @ngdoc directive
 * @name umbraco.directives.directive:fsTreeItem
 * @element li
 * @function
 *
 * @description
 * Renders a list item, representing a single node in the tree.
 * Includes element to toggle children, and a menu toggling button
 *
 * **note:** This directive is only used internally in the fsTree directive
 *
 * @example
   <example module="umbraco">
    <file name="index.html">
         <umb-tree-item ng-repeat="child in tree.children" node="child" callback="callback" section="content"></umb-tree-item>
    </file>
   </example>
 */
angular.module('umbraco.directives')
.directive('fsTreeItem', function ($compile, $http, $templateCache, $interpolate, $log, $location, $rootScope, $window, treeService, $timeout, localizationService) {
    return {
        restrict: 'E',
        replace: true,

        scope: {
            section: '@',
            eventhandler: '=',
            currentNode: '=',
            node: '=',
            tree: '='
        },

        //TODO: Remove more of the binding from this template and move the DOM manipulation to be manually done in the link function,
        // this will greatly improve performance since there's potentially a lot of nodes being rendered = a LOT of watches!

        template: '<li ng-class="{\'current\': (node == currentNode)}" on-right-click="altSelect(node, $event)">' +
            '<div ng-class="getNodeCssClass(node)" ng-swipe-right="options(node, $event)" >' +
            //NOTE: This ins element is used to display the search icon if the node is a container/listview and the tree is currently in dialog
            //'<ins ng-if="tree.enablelistviewsearch && node.metaData.isContainer" class="umb-tree-node-search icon-search" ng-click="searchNode(node, $event)" alt="searchAltText"></ins>' + 
            '<ins ng-class="{\'icon-navigation-right\': !node.expanded, \'icon-navigation-down\': node.expanded}" ng-click="load(node)"></ins>' +
            '<i class="icon umb-tree-icon sprTree"></i>' +
            '<a class="fs-click" ng-click="select(node, $event)"></a>' +
            //NOTE: These are the 'option' elipses
            '<a class="fs-options umb-options" ng-click="options(node, $event)"><span class="umb-options"><i></i><i></i><i></i></span></a>' +
            '<div ng-show="node.loading" class="l"><div></div></div>' +
            '</div>' +
            '</li>',

        link: function (scope, element, attrs) {

            localizationService.localize('general_search').then(function (value) {
                scope.searchAltText = value;
            });

            //flag to enable/disable delete animations, default for an item is true
            var deleteAnimations = true;

            // Helper function to emit tree events
            function emitEvent(eventName, args) {

                if (scope.eventhandler) {
                    $(scope.eventhandler).trigger(eventName, args);
                }
            }

            // updates the node's DOM/styles
            function setupNodeDom(node, tree) {

                //get the first div element
                element.children(':first')
                    //set the padding
                    .css('padding-left', (node.level * 20) + 'px');

                //remove first 'ins' if there is no children
                //show/hide last 'ins' depending on children
                if (!node.hasChildren) {
                    element.find('ins:first').remove();
                    element.find('ins').last().hide();
                }
                else {
                    element.find('ins').last().show();
                }

                var icon = element.find('i:first');
                icon.addClass(node.cssClass);
                icon.attr('title', node.routePath);

                if (node.name.match(/\.(gif|jpg|jpeg|tiff|png)$/i)) {
                    icon.hide();
                    var thumbnailPath = '/umbraco/backoffice/FileSystemPicker/FileSystemThumbnailApi/GetThumbnail?width=150&amp;imagePath=' + node.id;
                    element.find('a:first').append($('<img src="' + thumbnailPath + '" /><p>' + node.name + '</p>'));
                } else {
                    element.find('a:first').text(node.name);
                }

                if (node.icon === 'icon-document') {
                    //element.find('a.fs-options').remove();
                }

                if (node.style) {
                    element.find('i:first').attr('style', node.style);
                }
            }

            //This will deleteAnimations to true after the current digest
            function enableDeleteAnimations() {
                //do timeout so that it re-enables them after this digest
                $timeout(function () {
                    //enable delete animations
                    deleteAnimations = true;
                }, 0, false);
            }

            /** Returns the css classses assigned to the node (div element) */
            scope.getNodeCssClass = function (node) {
                if (!node) {
                    return '';
                }
                var css = [];
                if (node.cssClasses) {
                    _.each(node.cssClasses, function (c) {
                        css.push(c);
                    });
                }
                if (node.selected) {
                    css.push('umb-tree-node-checked');
                }
                return css.join(' ');
            };

            //add a method to the node which we can use to call to update the node data if we need to ,
            // this is done by sync tree, we don't want to add a $watch for each node as that would be crazy insane slow
            // so we have to do this
            scope.node.updateNodeData = function (newNode) {
                _.extend(scope.node, newNode);
                //now update the styles
                setupNodeDom(scope.node, scope.tree);
            };

            scope.node.refresh = function () {
                treeService.loadNodeChildren({ node: scope.node, section: scope.section })
                    .then(function (data) {
                        //emit expanded event
                        emitEvent('treeNodeExpanded', { tree: scope.tree, node: scope.node, children: data });
                        enableDeleteAnimations();
                    });
            };

            /**
              Method called when the options button next to a node is called
              In the main tree this opens the menu, but internally the tree doesnt
              know about this, so it simply raises an event to tell the parent controller
              about it.
            */
            scope.options = function (n, ev) {
                emitEvent('treeOptionsClick', { element: element, tree: scope.tree, node: n, event: ev });
            };

            /**
              Method called when an item is clicked in the tree, this passes the 
              DOM element, the tree node object and the original click
              and emits it as a treeNodeSelect element if there is a callback object
              defined on the tree
            */
            scope.select = function (n, ev) {
                emitEvent('treeNodeSelect', { element: element, tree: scope.tree, node: n, event: ev });
            };

            /**
              Method called when an item is right-clicked in the tree, this passes the 
              DOM element, the tree node object and the original click
              and emits it as a treeNodeSelect element if there is a callback object
              defined on the tree
            */
            scope.altSelect = function (n, ev) {
                emitEvent('treeNodeAltSelect', { element: element, tree: scope.tree, node: n, event: ev });
            };

            /** method to set the current animation for the node. 
            *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
            *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
            */
            scope.animation = function () {
                if (scope.node.showHideAnimation) {
                    return scope.node.showHideAnimation;
                }
                if (deleteAnimations && scope.node.expanded) {
                    return { leave: 'tree-node-delete-leave' };
                }
                else {
                    return {};
                }
            };

            /**
              Method called when a node in the tree is expanded, when clicking the arrow
              takes the arrow DOM element and node data as parameters
              emits treeNodeCollapsing event if already expanded and treeNodeExpanding if collapsed
            */
            scope.load = function (node) {
                if (node.expanded) {
                    deleteAnimations = false;
                    emitEvent('treeNodeCollapsing', { tree: scope.tree, node: node, element: element });
                    node.expanded = false;
                }
                else {
                    scope.loadChildren(node, false);
                }
            };

            /* helper to force reloading children of a tree node */
            scope.loadChildren = function (node, forceReload) {
                //emit treeNodeExpanding event, if a callback object is set on the tree
                emitEvent('treeNodeExpanding', { tree: scope.tree, node: node });

                if (node.hasChildren && (forceReload || !node.children || (angular.isArray(node.children) && node.children.length === 0))) {
                    //get the children from the tree service
                    treeService.loadNodeChildren({ node: node, section: scope.section })
                        .then(function (data) {
                            //emit expanded event
                            emitEvent('treeNodeExpanded', { tree: scope.tree, node: node, children: data });
                            enableDeleteAnimations();
                        });
                }
                else {
                    emitEvent('treeNodeExpanded', { tree: scope.tree, node: node, children: node.children });
                    node.expanded = true;
                    enableDeleteAnimations();
                }
            };

            //if the current path contains the node id, we will auto-expand the tree item children

            setupNodeDom(scope.node, scope.tree);

            var template = '<ul ng-class="{collapsed: !node.expanded}"><fs-tree-item ng-repeat="child in node.children" eventhandler="eventhandler" tree="tree" current-node="currentNode" node="child" section="{{section}}" ng-animate="animation()"></fs-tree-item></ul>';
            var newElement = angular.element(template);
            $compile(newElement)(scope);
            element.append(newElement);

        }
    };
});

angular.module('umbraco.directives')
.directive('umbUploadPreview', function ($parse) {
    return {
        link: function (scope, element, attr, ctrl) {
            var fn = $parse(attr.umbUploadPreview),
                file = fn(scope);
            if (file.preview) {
                element.append(file.preview);
            }
        }
    };
})
.directive('fsFileUpload', function ($rootScope, assetsService, $timeout, $log, umbRequestHelper, mediaResource, imageHelper, notificationsService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: '/App_Plugins/FileSystemPicker/filesystem-picker-file-upload.html',
        scope: {
            options: '=',
            node: '=',
            onUploadComplete: '='
        },
        link: function (scope, element, attrs) {
            //NOTE: Blueimp handlers are documented here: https://github.com/blueimp/jQuery-File-Upload/wiki/Options
            //NOTE: We are using a Blueimp version specifically ~9.4.0 because any higher than that and we get crazy errors with jquery, also note
            // that the jquery UI version 1.10.3 is required for this blueimp version! if we go higher to 1.10.4 it breaks! seriously! 
            // It's do to with the widget framework in jquery ui changes which must have broken a whole lot of stuff. So don't change it for now.

            if (scope.onUploadComplete && !angular.isFunction(scope.onUploadComplete)) {
                throw 'onUploadComplete must be a function callback';
            }

            scope.uploading = false;
            scope.files = [];
            scope.progress = 0;

            var defaultOptions = {
                url: '/umbraco/backoffice/FileSystemPicker/FileSystemPickerApi/PostAddFile?origin=blueimp',
                //we'll start it manually to make sure the UI is all in order before processing
                autoUpload: true,
                disableImageResize: /Android(?!.*Chrome)|Opera/
                    .test(window.navigator.userAgent),
                previewMaxWidth: 150,
                previewMaxHeight: 150,
                previewCrop: true,
                dropZone: element.find('.drop-zone'),
                fileInput: element.find('input.uploader'),
                formData: {
                    currentFolder: scope.node.id === '-1' ? '~/' + scope.node.metaData.startfolder : '~/' + scope.node.id + '/'
                }
            };

            //merge options
            scope.blueimpOptions = angular.extend(defaultOptions, scope.options);

            function loadChildren(node) {

            }

            function checkComplete(e, data) {
                scope.$apply(function () {
                    //remove the amount of files complete
                    //NOTE: function is here instead of in the loop otherwise jshint blows up
                    function findFile(file) { return file === data.files[i]; }
                    for (var i = 0; i < data.files.length; i++) {
                        var found = _.find(scope.files, findFile);
                        found.completed = true;
                    }

                    //Show notifications!!!!
                    if (data.result && data.result.notifications && angular.isArray(data.result.notifications)) {
                        for (var n = 0; n < data.result.notifications.length; n++) {
                            notificationsService.showNotification(data.result.notifications[n]);
                        }
                    }

                    //when none are left resync everything
                    var remaining = _.filter(scope.files, function (file) { return file.completed !== true; });
                    if (remaining.length === 0) {

                        scope.progress = 100;

                        //just the ui transition isn't too abrupt, just wait a little here
                        $timeout(function () {
                            scope.progress = 0;
                            scope.files = [];
                            scope.uploading = false;

                            loadChildren(scope.node);

                            data.node = scope.node;

                            //call the callback
                            scope.onUploadComplete.apply(this, [data]);


                        }, 200);


                    }
                });
            }

            //when one is finished
            scope.$on('fileuploaddone', function (e, data) {
                checkComplete(e, data);
            });

            //This handler gives us access to the file 'preview', this is the only handler that makes this available for whatever reason
            // so we'll use this to also perform the adding of files to our collection
            scope.$on('fileuploadprocessalways', function (e, data) {
                scope.$apply(function () {
                    scope.uploading = true;
                    scope.files.push(data.files[data.index]);
                });
            });

            //This is a bit of a hack to check for server errors, currently if there's a non
            //known server error we will tell them to check the logs, otherwise we'll specifically 
            //check for the file size error which can only be done with dodgy string checking
            scope.$on('fileuploadfail', function (e, data) {
                if (data.jqXHR.status === 500 && data.jqXHR.responseText.indexOf('Maximum request length exceeded') >= 0) {
                    notificationsService.error(data.errorThrown, 'The uploaded file was too large, check with your site administrator to adjust the maximum size allowed');

                }
                else {
                    notificationsService.error(data.errorThrown, data.jqXHR.statusText);
                }

                checkComplete(e, data);
            });

            //This executes prior to the whole processing which we can use to get the UI going faster,
            //this also gives us the start callback to invoke to kick of the whole thing
            scope.$on('fileuploadadd', function (e, data) {
                scope.$apply(function () {
                    scope.uploading = true;
                });
            });

            // All these sit-ups are to add dropzone area and make sure it gets removed if dragging is aborted! 
            scope.$on('fileuploaddragover', function (e, data) {
                if (!scope.dragClearTimeout) {
                    scope.$apply(function () {
                        scope.dropping = true;
                    });
                }
                else {
                    $timeout.cancel(scope.dragClearTimeout);
                }
                scope.dragClearTimeout = $timeout(function () {
                    scope.dropping = null;
                    scope.dragClearTimeout = null;
                }, 300);
            });

            //init load
            loadChildren(scope.node);

        }
    };
});