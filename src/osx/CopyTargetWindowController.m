/***************************************************************************
 *  CopyTargetWindowController.m
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

#import "CopyTargetWindowController.h"
#import "GiverNetService.h"

@implementation CopyTargetWindowController

static CopyTargetWindowController *sharedInstance = nil;


+ (CopyTargetWindowController *)sharedInstance
{
	if(sharedInstance == nil)
	{
		sharedInstance = [[CopyTargetWindowController alloc] initWithWindowNibName:@"CopyTargetWindow"];
	}

    return sharedInstance;
}

- (void)windowWillClose:(NSNotification *)aNotification
{
	if(sharedInstance != nil)
	{
		[sharedInstance release];
		sharedInstance = nil;	
	}
}


-(void)awakeFromNib
{
	filesArray = [[NSMutableArray array] retain];
	filesArrayController = [[NSArrayController alloc] initWithContent:filesArray];

    NSMutableDictionary *bindingOptions = [NSMutableDictionary dictionary];
    	
	[bindingOptions setObject:[NSNumber numberWithBool:NO] forKey:@"NSNullPlaceholder"];
	[bindingOptions setObject:[NSNumber numberWithBool:YES] forKey:@"NSContinuouslyUpdatesValueBindingOption"];
	[bindingOptions setObject:[NSNumber numberWithBool:YES] forKey:@"NSValidatesImmediatelyBindingOption"];
	[bindingOptions setObject:[NSNumber numberWithBool:YES] forKey:@"NSConditionallySetsEnabledBindingOption"];

	[compTableColumn bind:@"value" toObject:[[GiverNetService sharedInstance] servicesController]
					withKeyPath:@"arrangedObjects.name" options:bindingOptions];
	[fileCopyColumn bind:@"value" toObject:filesArrayController
					withKeyPath:@"arrangedObjects" options:bindingOptions];

}

- (NSArrayController *)copyFilesController
{
	return filesArrayController;
}

- (IBAction)cancel:(id)sender
{
	[self close];
}

- (IBAction)copyFiles:(id)sender
{
}

@end
