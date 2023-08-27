---
layout: post
title: "Recurrent tasks"
date: 2023-03-07
comments: true
categories: [recurrent,tasks]
---
Recurrent tasks are almost always bad conceptualized in task management applications. Typically, you create a task and then set a recurrence configuration for that task, for example once a week. What you see in your task list is a single task. Once you mark it as completed, a new task appears which due date is a week later.

- [ ] 2019-03-07 Do laundry (recurrence: 1 week)


- [X] ~~2019-03-07 Do laundry (recurrence: 1 week)~~

- [ ] 2019-03-14 Do laundry (recurrence: 1 week)

But this representation of a recurrent task is wrong. The second task was conceptually there before you completed the first task, but you don't get to see it.

You should be able to see all the recurring instances, the same way you do in, e.g., Google Calendar.

- [X] ~~2019-03-07 Do laundry (recurrence: 1 week)~~

- [ ] 2019-03-14 Do laundry (recurrence: 1 week)
- [ ] 2019-03-21 Do laundry (recurrence: 1 week)
- [ ] 2019-03-28 Do laundry (recurrence: 1 week)
- ...

Edition should be also similar to Google Calendar.

But this approach poses some problems when we consider some visualizations of our task lists. For example, the typical task app is able to show a list of tasks sorted alphabetically. Strictly, when you set an unbounded recurrence, you have an infinite set of tasks which title is identical. So when you sort by anything but a date, it's not possible to show them all. We have to revert again to showing the abbreviated definition of a recurring task. But it's not the same to show a definition of the recurring task and to show the next instance of that recurring task.