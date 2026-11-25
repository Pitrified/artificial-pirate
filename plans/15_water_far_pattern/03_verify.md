---
status: done
---

# Phase 03 - Verify

## Overview

The phone's eye.
Context: [`00_start.md`](00_start.md).

## Goals

1. The user's read in the chase camera behind the giant.
2. A diagnostics report: the GPU time against the fleet's 12.8 ms p50.

## Plan

- A dev APK; the user's run and read.

## Done when

- The user's read and report.

## What the implementation found

- The user's read of 7f297aa on the Pixel 7 Pro: looks good.
- Diagnostics, 91 s with both giants fighting: 60 fps held, GPU p50 12.3 ms and p95 13.8 ms (the fleet's run: p50 12.8 ms), 5 slow frames; the two extra texture reads cost nothing the phone shows.
