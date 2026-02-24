(function () {
  "use strict";

  function DictionaryTranslateController($http, umbRequestHelper, notificationsService) {
    var vm = this;

    vm.sourceIso2 = "en";
    vm.onlyFillMissing = true;
    vm.delayMs = 50;

    vm.running = false;
    vm.lastResult = null;

    vm.auditRoot = "Views";
    vm.auditRunning = false;
    vm.auditResult = null;

    vm.createEnglishValueFromKey = true;
    vm.createRunning = false;
    vm.createResult = null;

    vm.deleteDryRun = true;
    vm.deleteOnlyLeaf = true;
    vm.deleteRunning = false;
    vm.deleteResult = null;

    vm.run = function () {
      vm.running = true;
      vm.lastResult = null;

      var payload = {
        sourceIso2: (vm.sourceIso2 || "en").toLowerCase(),
        onlyFillMissing: !!vm.onlyFillMissing,
        delayMs: Number(vm.delayMs || 0)
      };

      // This matches your UmbracoAuthorizedApiController route:
      // POST /umbraco/backoffice/AiTools/DictionaryAutoTranslate/Run
      var url = "/umbraco/backoffice/AiTools/DictionaryAutoTranslate/Run";

      umbRequestHelper
        .resourcePromise($http.post(url, payload), "AI translate failed")
        .then(function (res) {
          vm.lastResult = res.data;
          notificationsService.success("AI Tools", "Done");
        })
        .catch(function (err) {
          notificationsService.error("AI Tools", err && err.data ? JSON.stringify(err.data) : "Error");
        })
        .finally(function () {
          vm.running = false;
        });
    };

    vm.runAudit = function () {
      vm.auditRunning = true;
      vm.auditResult = null;

      var url = "/umbraco/backoffice/AiTools/DictionaryAudit/Scan";

      return umbRequestHelper
        .resourcePromise(
          $http.get(url, {
            params: {
              root: vm.auditRoot
            }
          }),
          "Failed to run dictionary audit"
        )
        .then(function (res) {
          // Some Umbraco versions return the raw JSON in res.data; others return it directly.
          vm.auditResult = res && res.data ? res.data : res;

          if (vm.auditResult && vm.auditResult.MissingKeys === 0) {
            notificationsService.success("Dictionary audit", "No missing keys found");
          } else {
            notificationsService.warning("Dictionary audit", "Missing keys found");
          }
        })
        .catch(function (err) {
          notificationsService.error(
            "Dictionary audit failed",
            err && err.data ? JSON.stringify(err.data) : "Error"
          );
        })
        .finally(function () {
          vm.auditRunning = false;
        });
    };

    vm.createMissing = function () {
      vm.createRunning = true;
      vm.createResult = null;

      var url = "/umbraco/backoffice/AiTools/DictionaryAudit/CreateMissing";
      var payload = {
        root: vm.auditRoot,
        defaultValueFromKey: false,
        setEnglishValueFromKey: !!vm.createEnglishValueFromKey,
        englishIso2: "en",
        parentId: null
      };

      return umbRequestHelper
        .resourcePromise($http.post(url, payload), "Failed to create missing dictionary keys")
        .then(function (res) {
          vm.createResult = res && res.data ? res.data : res;
          notificationsService.success("Dictionary audit", "Created missing keys");

          // Refresh the audit view so counts update.
          return vm.runAudit();
        })
        .catch(function (err) {
          notificationsService.error(
            "Create missing keys failed",
            err && err.data ? JSON.stringify(err.data) : "Error"
          );
        })
        .finally(function () {
          vm.createRunning = false;
        });
    };

    vm.deleteUnused = function () {
      vm.deleteRunning = true;
      vm.deleteResult = null;

      var url = "/umbraco/backoffice/AiTools/DictionaryAudit/DeleteUnused";
      var payload = {
        root: vm.auditRoot,
        underParentId: null,
        dryRun: !!vm.deleteDryRun,
        onlyLeaf: !!vm.deleteOnlyLeaf
      };

      return umbRequestHelper
        .resourcePromise($http.post(url, payload), "Failed to delete unused dictionary items")
        .then(function (res) {
          vm.deleteResult = res && res.data ? res.data : res;

          if (vm.deleteDryRun) {
            notificationsService.success("Dictionary cleanup", "Preview ready");
          } else {
            notificationsService.success("Dictionary cleanup", "Delete completed");
          }
        })
        .catch(function (err) {
          notificationsService.error(
            "Dictionary cleanup failed",
            err && err.data ? JSON.stringify(err.data) : "Error"
          );
        })
        .finally(function () {
          vm.deleteRunning = false;
        });
    };
  }

  angular.module("umbraco").controller("AiTools.DictionaryTranslateController", DictionaryTranslateController);
})();
