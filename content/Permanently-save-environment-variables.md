+++
title = "Permanently save environment variables"
date = 2019-04-15
[taxonomies]
tags = ["linux"]
categories = ["administration"]
+++
Environment variables are only stored temporally if you are setting them using terminal:
<!-- more -->
```bash
export TEST=/home/test/
```
<!-- more -->
To save this variable for all users, need to add this command to `/etc/profile`. If you want to have this environment variable only for current user, it is needed to add this command to `~/.profile`.

Thanks.