magick `
-size 340x340 `
canvas:none `
-background white `
-alpha remove `
-background gray `
-vignette 0x100+0+0 `
-fill "rgb(255,240,100)" -colorize 20 `
-channel RGB -function polynomial 0.6,0.4 `
-attenuate 0.3 +noise Gaussian `
"..\src\CommandLine\input\assets\img\score_background.png"
