---
layout: post
title: "TFS 2013 Checkout override with a git branch"
date: 2015-02-06
comments: true
categories: [tfs,git]
---
Another quick note on TFS + git. When you start a manual build you can specify what commit you want in a number of ways, using the "Checkout override" option. You can specify either a commit, a tag or a branch.

For the commit you paste the SHA-1 (or a significant part), just as you do with the command line.

For the tag you use the tag name.

For the branch just **one trick**: you must prepend `origin/` to your branch name.