angular.module('app.routes', [])

.config(function($stateProvider, $urlRouterProvider) {

  // Ionic uses AngularUI Router which uses the concept of states
  // Learn more here: https://github.com/angular-ui/ui-router
  // Set up the various states which the app can be in.
  // Each state's controller can be found in controllers.js
  $stateProvider
    

  .state('tabsController.page2', {
    url: '/index',
    views: {
      'tab1': {
          templateUrl: '/Vote/Mobile/page2.html',
        controller: 'page2Ctrl'
      }
    }
  })

  .state('tabsController.page3', {
    url: '/order',
    views: {
      'tab2': {
          templateUrl: '/Vote/Mobile/page3.html',
        controller: 'page3Ctrl'
      }
    }
  })

  .state('tabsController.page4', {
    url: '/about',
    views: {
      'tab3': {
          templateUrl: '/Vote/Mobile/page4.html',
        controller: 'page4Ctrl'
      }
    }
  })

  .state('tabsController', {
    url: '/page1',
      templateUrl: '/Vote/Mobile/tabsController.html',
    abstract:true
  })

  .state('login', {
    url: '/login',
      templateUrl: '/Vote/Mobile/login.html',
    controller: 'loginCtrl'
  })

  .state('signup', {
    url: '/signup',
      templateUrl: '/Vote/Mobile/signup.html',
    controller: 'signupCtrl'
  })

  .state('tabsController.page7', {
    url: '/item',
    views: {
      'tab1': {
          templateUrl: '/Vote/Mobile/page7.html',
        controller: 'page7Ctrl'
      }
    }
  })

  .state('tEST', {
    url: '/page8',
      templateUrl: '/Vote/Mobile/tEST.html',
    controller: 'tESTCtrl'
  })

$urlRouterProvider.otherwise('/index')


});