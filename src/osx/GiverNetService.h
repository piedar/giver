/***************************************************************************
 *  GiverNetService.h
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

#import <Cocoa/Cocoa.h>
#include "mdns_helper.h"

@interface GiverNetService : NSObject 
{
	struct mdns_data_struct *data;
//    NSNetService * netService;
    NSFileHandle * listeningSocket;
	
	NSNumber *isSharing;
	NSString	*serviceDescription;
	NSString	*serviceShortString;
	NSString	*serviceLongString;
	NSString	*serviceControlString;

//	NSNetServiceBrowser * browser;
    NSMutableArray * services;
	NSArrayController *servicesArrayController;
}

+ (GiverNetService *)sharedInstance;

- (NSArrayController *)servicesController;

- (IBAction)toggleSharing:(id)sender;
- (IBAction)stop:(id)sender;

-(NSNumber *)sharingEnabled;
-(void)setSharingEnabled:(NSNumber *)value;

-(NSString *)serviceName;
-(void)setServiceName:(NSString *)name;

-(NSString *)serviceShortString;
-(void)setServiceShortString:(NSString *)shortString;

-(NSString *)serviceLongString;
-(void)setServiceLongString:(NSString *)longString;

-(NSString *)controlString;
-(void)setControlString:(NSString *)controlString;



@end
