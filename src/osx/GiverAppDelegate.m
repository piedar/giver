/***************************************************************************
 *  GiverAppDelegate.m
 *
 *  Copyright (C) 2007 Calvin Gaisford <calvinrg@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

#import "GiverAppDelegate.h"
#import "GiverWindowController.h"
#import "GiverNetService.h"
#import "CopyTargetWindowController.h"


@implementation GiverAppDelegate

//===================================================================
// applicationDidFinishLaunching
// Application Delegate method, called when the application is done
// loading
//===================================================================
- (void)applicationDidFinishLaunching:(NSNotification*)notification
{
	[self showGiverWindow:self];
}


- (NSApplicationTerminateReply) applicationShouldTerminate:(NSApplication *)sender
{
	[[GiverNetService sharedInstance] stop:self];
    return NSTerminateNow;
}




- (IBAction)showGiverWindow:(id)sender
{
	[[GiverWindowController sharedInstance] showWindow:self];
}


// NSApplication delegate methods to respond to files dropped on icon
- (BOOL)application:(NSApplication *)sender openFile:(NSString *)filename
{
	NSLog(@"Called to open file: %@", filename);
	[[CopyTargetWindowController sharedInstance] showWindow:self];
	[[[CopyTargetWindowController sharedInstance] copyFilesController] addObject:filename];
	return NO;
}

- (void)application:(NSApplication *)sender openFiles:(NSArray *)filenames
{
	NSLog(@"Called to open multiple files");
	[[CopyTargetWindowController sharedInstance] showWindow:self];
	[[[CopyTargetWindowController sharedInstance] copyFilesController] addObjects:filenames];
}


@end
