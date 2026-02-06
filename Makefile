SHELL := /bin/bash

UNITY_PATH ?= /Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity
RESULTS_PATH ?= $(CURDIR)/TestResults.xml
COVERAGE_PATH ?= $(CURDIR)/CoverageResults

.PHONY: test test-play test-cover

test:
	UNITY_PATH=$(UNITY_PATH) RESULTS_PATH=$(RESULTS_PATH) TEST_PLATFORM=editmode ./scripts/run-tests.sh

test-play:
	UNITY_PATH=$(UNITY_PATH) RESULTS_PATH=$(RESULTS_PATH) TEST_PLATFORM=playmode ./scripts/run-tests.sh

test-cover:
	UNITY_PATH=$(UNITY_PATH) RESULTS_PATH=$(RESULTS_PATH) COVERAGE_PATH=$(COVERAGE_PATH) CODE_COVERAGE=1 TEST_PLATFORM=editmode ./scripts/run-tests.sh
