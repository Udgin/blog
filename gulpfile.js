var gulp = require('gulp');
var marked = require('gulp-marked');
var replace = require('gulp-replace');
var rename = require('gulp-rename');
var fs = require('fs');

var articlePath = './article/';
var tempPath = './temp/';

gulp.task('splitmarkdown', function(){
    fs.readdirSync(tempPath).forEach(function(file,index) {
        var curPath = tempPath + file;
        fs.unlinkSync(curPath);
    });
    var body = fs.readFileSync('blog.md', 'utf8'); 
    var bodies = body.split("---\n");
    var name = 0;
    bodies.map(function(val) {
        var title = val.replace(/^\s+|\s+$/g, '');
        var nameOfFile = title
            .substring(3, title.indexOf('\n'))
            .replace(/\"/g, '\'')
            .replace(/[\/\,-]/g, '')
            .replace(/[\s]/g, '_')
            .replace(/_+/g, '_');
        fs.writeFileSync(tempPath+ nameOfFile + '.md', val);
    });
});

gulp.task('markdownv2', ['splitmarkdown'], function () {
    return gulp.src(tempPath +'*.md')
        .pipe(marked())
        .pipe(gulp.dest(tempPath));
});

gulp.task('replaceArticle', ['markdownv2'], function () {
    fs.readdirSync(articlePath).forEach(function(file,index) {
        var curPath = articlePath + file;
        fs.unlinkSync(curPath);
    });
    var template = fs.readFileSync('articleTemplate.html', 'utf8');
    fs.readdirSync(tempPath).forEach(function(file,index) {
        if (file.endsWith('html')){
            var curPath = tempPath + file;
            var val = fs.readFileSync(curPath, 'utf8'); 
            fs.writeFileSync(articlePath + file, template.replace(/\{BODY\}/g, val));
        }
    });
});

gulp.task('indexBody', ['replaceArticle'], function(){
    var indexBody = '';
    fs.readdirSync(articlePath).forEach(function(file,index) {
        if (file.endsWith('html')){
            var curPath = articlePath + file;
            var linkText = file.replace(/\.html$/g, '').replace(/_/g, ' ');
            indexBody += '<div><a href="'+curPath+'">'+linkText + '</a></div>';
        }
    });
    fs.writeFileSync('indexBody.html', indexBody);
});

gulp.task('replaceIndex', ['indexBody'], function () {
    return gulp.src('./indexTemplate.html')
        .pipe(replace(/{BODY}/, function (s) {
            var body = fs.readFileSync('indexBody.html', 'utf8');
            return body;
        }))
        .pipe(rename('./index.html'))
        .pipe(gulp.dest('./'));
});

gulp.task('v2', ['splitmarkdown', 'markdownv2', 'replaceArticle', 'indexBody', 'replaceIndex']);


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
    gulp.watch(['./blog.md', './template.html'], ['markdown', 'replace']);
});