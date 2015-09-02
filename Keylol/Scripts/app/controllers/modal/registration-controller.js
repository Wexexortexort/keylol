﻿(function() {
	"use strict";

	keylolApp.controller("RegistrationController", [
		"$scope", "close", "$http", "utils",
		function($scope, close, $http, utils) {
			$scope.vm = {
				IdCode: "",
				UserName: "",
				Password: "",
				ConfirmPassword: "",
				Email: ""
			};
			var geetestResult;
			var gee;
			$scope.geetestId = utils.createGeetest("float", function(result, geetest) {
				gee = geetest;
				geetestResult = result;
				$scope.vm.GeetestChallenge = geetestResult.geetest_challenge;
				$scope.vm.GeetestSeccode = geetestResult.geetest_seccode;
				$scope.vm.GeetestValidate = geetestResult.geetest_validate;
			});
			$scope.error = {};
			$scope.errorDetect = utils.modelErrorDetect;
			$scope.cancel = function() {
				close();
			};
			$scope.submit = function(form) {
				$scope.error = {};
				$scope.vm.IdCode = $scope.vm.IdCode.toUpperCase();
				utils.modelValidate.idCode($scope.vm.IdCode, $scope.error, "vm.IdCode");
				utils.modelValidate.username($scope.vm.UserName, $scope.error, "vm.UserName");
				if (utils.modelValidate.password($scope.vm.Password, $scope.error, "vm.Password")) {
					if ($scope.vm.Password !== $scope.vm.ConfirmPassword) {
						$scope.error["vm.ConfirmPassword"] = "not match";
					}
				}
				if (form.email.$invalid) {
					$scope.error["vm.Email"] = "Email is invalid.";
				} else if (!$scope.vm.Email) {
					$scope.error["vm.Email"] = "Email cannot be empty.";
				}
				if (typeof geetestResult === "undefined") {
					$scope.error.authCode = true;
				}
				if (!$.isEmptyObject($scope.error))
					return;
				$http.post("/api/user", $scope.vm)
					.then(function(response) {
						alert("注册成功");
					}, function(response) {
						switch (response.status) {
						case 400:
							$scope.error = response.data.ModelState;
							if ($scope.error.authCode) {
								gee.refresh();
							}
							break;
						default:
							console.error(response.data);
						}
					});
			};
		}
	]);
})();