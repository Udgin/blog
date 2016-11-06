var gulp = require('gulp');
var marked = require('gulp-marked');
var replace = require('gulp-replace');
var rename = require('gulp-rename');
var fs = require('fs');


gulp.task('markdown', function () {
    return gulp.src('./blog.md')
        .pipe(marked())
        .pipe(gulp.dest('./'));
});

gulp.task('replace', function () {
    return gulp.src('./template.html')
        .pipe(replace(/{BODY}/, function (s) {
            var body = fs.readFileSync('blog.html', 'utf8');
            return body;
        }))
        .pipe(rename('./index.html'))
        .pipe(gulp.dest('./'));
});

gulp.task('default', ['markdown', 'replace'], function () {
    gulp.watch('./blog.md', ['markdown', 'replace']);
});