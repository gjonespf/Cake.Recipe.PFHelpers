# Gitversion and Jenkins not playing nice...
rm -Rf .git/gitversion_cache/
git fetch origin master:master
git fetch origin develop:develop
