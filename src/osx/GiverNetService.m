/***************************************************************************
 *  GiverNetService.m
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
 

#import "GiverNetService.h"
#import "GiverAppDelegate.h"
#include "mdns_helper.h"

// imports required for socket initialization
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include <dns_sd.h>


static GiverNetService *sharedInstance = nil;


void DNSSD_API service_browser_callback(	
							DNSServiceRef		sdRef, 
							DNSServiceFlags		flags,
							uint32_t			interfaceIndex, 
							DNSServiceErrorType	errorCode,
							const char *		name,
							const char *		type,
							const char *		domain, 
							void *				context );
void DNSSD_API service_resolve_callback(
								DNSServiceRef		sdRef, 
								DNSServiceFlags		flags, 
								uint32_t			interfaceIndex, 
								DNSServiceErrorType	errorCode, 
								const char			*fullname, 
								const char			*hosttarget, 
								uint16_t			port, 
								uint16_t			txtLen, 
								const char			*txtRecord, 
								void				*context );


@implementation GiverNetService

//===================================================================
// init
// Initialize the iFolderData
//===================================================================
- (id)init 
{
	if (sharedInstance) 
	{
		[self dealloc];
	} 
	else
	{
		DNSServiceErrorType err;
		
		sharedInstance = [super init];
		isSharing = [[NSNumber alloc] initWithBool:NO];
		serviceDescription = [NSString stringWithFormat:@"%@@%@", NSUserName(), [[NSProcessInfo processInfo] hostName]];
		serviceShortString = [NSString stringWithFormat:@"Giver is off"];
		serviceLongString = [NSString stringWithFormat:@"Click Start to turn on Giver and allow other users to see your computer in their available computers list."];
		serviceControlString = [NSString stringWithFormat:@"Start"];

		gLog1(@"Starting up the DNSServiceBrowse...");
		err = mdns_browser_init(&data, service_browser_callback);
								
		assert(err == kDNSServiceErr_NoError );

		gLog1(@"DNSServiceBrowse is up and running");
		
		// err = WSAAsyncSelect( (SOCKET) DNSServiceRefSockFD( gServiceRef ), wind, BONJOUR_EVENT, FD_READ | FD_CLOSE );
		// assert( err == kDNSServiceErr_NoError );		
		
//		browser = [[NSNetServiceBrowser alloc] init];
		services = [[NSMutableArray array] retain];
		servicesArrayController = [[NSArrayController alloc] initWithContent:services];
//		[browser setDelegate:self];
		
		// Passing in "" for the domain causes us to browse in the default browse domain, which currently will always be the ".local" domain.
//		[browser searchForServicesOfType:@"_giver._tcp." inDomain:@""];
		
		//[GiverNetService exposeBinding:@"sharingEnabled"];
	}
	return sharedInstance;
}




//===================================================================
// dealloc
// free up the iFolderData
//===================================================================
-(void) dealloc
{
	[super dealloc];
	gLog1(@"Shutting down the DNSServiceBrowse...");	
	mdns_browser_free(&data);
}




//===================================================================
// sharedInstance
// get the single instance for the app
//===================================================================
+ (GiverNetService *)sharedInstance
{
    return sharedInstance ? sharedInstance : [[self alloc] init];
}




- (IBAction)stop:(id)sender
{
/*
	if(netService) 
		[netService stop];
*/
}




- (IBAction)toggleSharing:(id)sender
{
/*    uint16_t chosenPort = 0;
    
	gLog1(@"Giver was asked to toggle");
    if(!listeningSocket) 
	{
        // Here, create the socket from traditional BSD socket calls, and then set up an NSFileHandle with that to listen for incoming connections.
        int fdForListening;
        struct sockaddr_in serverAddress;
        socklen_t namelen = sizeof(serverAddress);

        // In order to use NSFileHandle's acceptConnectionInBackgroundAndNotify method, we need to create a file descriptor that is itself a socket, bind that socket, and then set it up for listening. At this point, it's ready to be handed off to acceptConnectionInBackgroundAndNotify.
        if((fdForListening = socket(AF_INET, SOCK_STREAM, 0)) > 0) 
		{
            memset(&serverAddress, 0, sizeof(serverAddress));
            serverAddress.sin_family = AF_INET;
            serverAddress.sin_addr.s_addr = htonl(INADDR_ANY);
            serverAddress.sin_port = 0; // allows the kernel to choose the port for us.

            if(bind(fdForListening, (struct sockaddr *)&serverAddress, sizeof(serverAddress)) < 0) 
			{
                close(fdForListening);
                return;
            }

            // Find out what port number was chosen for us.
            if(getsockname(fdForListening, (struct sockaddr *)&serverAddress, &namelen) < 0) 
			{
                close(fdForListening);
                return;
            }

            chosenPort = ntohs(serverAddress.sin_port);
            
            if(listen(fdForListening, 1) == 0) 
			{
                listeningSocket = [[NSFileHandle alloc] initWithFileDescriptor:fdForListening closeOnDealloc:YES];
            }
        }
    }
    
    if(!netService) 
	{
        // lazily instantiate the NSNetService object that will advertise on our behalf.
        netService = [[NSNetService alloc] initWithDomain:@"" type:@"_giver._tcp." name:serviceDescription port:chosenPort];
        [netService setDelegate:self];
    }
    
    if(netService && listeningSocket) 
	{
        if([isSharing boolValue] == NO) 
		{
            [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(connectionReceived:) name:NSFileHandleConnectionAcceptedNotification object:listeningSocket];
            [listeningSocket acceptConnectionInBackgroundAndNotify];
            [netService publish];
        } 
		else 
		{
            [netService stop];
            [[NSNotificationCenter defaultCenter] removeObserver:self name:NSFileHandleConnectionAcceptedNotification object:listeningSocket];
            // There is at present no way to get an NSFileHandle to -stop- listening for events, so we'll just have to tear it down and recreate it the next time we need it.
            [listeningSocket release];
            listeningSocket = nil;
        }
    }
*/	
}

-(NSNumber *)sharingEnabled
{
	return isSharing;
}

-(void)setSharingEnabled:(NSNumber *)value
{
	isSharing = value;
}

- (NSArrayController *)servicesController
{
	return servicesArrayController;
}

-(NSString *)serviceName
{
	return serviceDescription;
}
-(void)setServiceName:(NSString *)name
{
	serviceDescription = name;
}

-(NSString *)serviceShortString
{
	return serviceShortString;
}
-(void)setServiceShortString:(NSString *)shortString
{
	serviceShortString = shortString;
}

-(NSString *)serviceLongString
{
	return serviceLongString;
}
-(void)setServiceLongString:(NSString *)longString
{
	serviceLongString = longString;
}

-(NSString *)controlString
{
	return serviceControlString;
}
-(void)setControlString:(NSString *)controlString
{
	serviceControlString = controlString;
}



void DNSSD_API service_browser_callback(	
							DNSServiceRef		sdRef, 
							DNSServiceFlags		flags,
							uint32_t			interfaceIndex, 
							DNSServiceErrorType	errorCode,
							const char *		name,
							const char *		type,
							const char *		domain, 
							void *				context )
{
	if( errorCode != kDNSServiceErr_NoError )
	{
		printf("MDNS Service Callback returned an error %d\n", errorCode);
		return;
	}
	
	if( flags & kDNSServiceFlagsAdd )
	{
		DNSServiceErrorType err;
		
		printf("Hey, we should add the record %s\n", name);
		err = mdns_browser_resolve(	
							interfaceIndex, 
							name, 
							type, 
							domain,
							service_resolve_callback);
		
/*
		if(givermdnsStruct->addClientCb != NULL)
		{
			(*(addClientCb)givermdnsStruct->addClientCb)
										(interfaceIndex, inName, inType, inDomain);
		}
*/
	}
	else
	{
		printf("A record should be removed: %s\n", name);
/*
		if(givermdnsStruct->delClientCb != NULL)
		{
			(*(addClientCb)givermdnsStruct->delClientCb)
										(interfaceIndex, name, type, domain);
		}
*/
	}
}




void DNSSD_API service_resolve_callback(
								DNSServiceRef		sdRef, 
								DNSServiceFlags		flags, 
								uint32_t			interfaceIndex, 
								DNSServiceErrorType	errorCode, 
								const char			*fullname, 
								const char			*hosttarget, 
								uint16_t			port, 
								uint16_t			txtLen, 
								const char			*txtRecord, 
								void				*context )
{
	if( errorCode != kDNSServiceErr_NoError )
	{
		printf("MDNS Service resolve Callback returned an error %d\n", errorCode);
		return;
	}
	
	printf("The resolved record is: name=%s, hosttarget=%s, port=%d, txtLen=%d, textRecord=%s\n",
							fullname, hosttarget, port, txtLen, txtRecord);
}







/*

// This object is the delegate of its NSNetServiceBrowser object. We're only interested in services-related methods, so that's what we'll call.
- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didFindService:(NSNetService *)aNetService moreComing:(BOOL)moreComing
{
	NSMutableDictionary *serviceDictionary = [[NSMutableDictionary alloc] init];
	
	[serviceDictionary setObject:aNetService forKey:@"service"];
	[serviceDictionary setObject:[aNetService name] forKey:@"name"];
	[servicesArrayController addObject:serviceDictionary];
	gLog2(@"Added the Service %@", [serviceDictionary valueForKey:@"name"]);
}


- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didRemoveService:(NSNetService *)aNetService moreComing:(BOOL)moreComing {
    // This case is slightly more complicated. We need to find the object in the list and remove it.
    NSEnumerator * enumerator = [[servicesArrayController arrangedObjects] objectEnumerator];
    NSDictionary * currentService;

	gLog2(@"Asked to remove the Service %@", [aNetService name]);

    while(currentService = [enumerator nextObject])
	{
        if ([[currentService valueForKey:@"service"] isEqual:aNetService])
		{
            [servicesArrayController removeObject:currentService];
            break;
        }
    }
}
*/ 





// This object is the delegate of its NSNetService. It should implement the NSNetServiceDelegateMethods that are relevant for publication (see NSNetServices.h).
/*
- (void)netServiceWillPublish:(NSNetService *)sender
{
	gLog1(@"Dude, it started");
	[self setValue:[NSNumber numberWithBool:YES] forKey:@"sharingEnabled"];
	[self setValue:[NSString stringWithFormat:@"Stop"] forKey:@"controlString"];
	[self setValue:[NSString stringWithFormat:@"Giver is on"] forKey:@"serviceShortString"];
	[self setValue:[NSString stringWithFormat:@"Click Stop to turn off Giver and remove your computer from being available on other computers."] forKey:@"serviceLongString"];

	
//	isSharing = [NSNumber numberWithBool:YES];
//	[[GiverWindowController sharedInstance] setSharingStatus:YES withMessage:@"Click Stop to turn off Giver."];
}



- (void)netService:(NSNetService *)sender didNotPublish:(NSDictionary *)errorDict
{
	[self setValue:[NSNumber numberWithBool:NO] forKey:@"sharingEnabled"];
	[self setValue:[NSString stringWithFormat:@"Start"] forKey:@"controlString"];
	[self setValue:[NSString stringWithFormat:@"Giver is off"] forKey:@"serviceShortString"];

    // Display some meaningful error message here, using the longerStatusText as the explanation.
    if([[errorDict objectForKey:NSNetServicesErrorCode] intValue] == NSNetServicesCollisionError) 
	{
		gLog1(@"Giver isn't too keen... you see... he's already got one.");
		[self setValue:[NSString stringWithFormat:@"A name collision occurred. A service is already running with that name someplace else. Click Start to turn on Giver."] forKey:@"serviceLongString"];
    } 
	else
	{
		gLog1(@"Dude, something went wrong.");
		[self setValue:[NSString stringWithFormat:@"An unknown error occurred. Click Start to turn on Giver."] forKey:@"serviceLongString"];
    }
	
    [listeningSocket release];
    listeningSocket = nil;
    [netService release];
    netService = nil;
}




- (void)netServiceDidStop:(NSNetService *)sender
{
	[self setValue:[NSNumber numberWithBool:NO] forKey:@"sharingEnabled"];
	[self setValue:[NSString stringWithFormat:@"Start"] forKey:@"controlString"];
	[self setValue:[NSString stringWithFormat:@"Giver is off"] forKey:@"serviceShortString"];
	[self setValue:[NSString stringWithFormat:@"Click Start to turn on Giver and allow other users to see your computer in their available computers list."] forKey:@"serviceLongString"];

	gLog1(@"Dude, Giver stopped.");
//	[[GiverWindowController sharedInstance] setSharingStatus:NO withMessage:@"Click Start to turn on Giver and allow other users to see your computer."];
    // We'll need to release the NSNetService sending this, since we want to recreate it in sync with the socket at the other end. Since there's only the one NSNetService in this application, we can just release it.
    [netService release];
    netService = nil;
}
*/




// This object is also listening for notifications from its NSFileHandle.
// When an incoming connection is seen by the listeningSocket object, we get the NSFileHandle representing the near end of the connection. We write the thumbnail image to this NSFileHandle instance.
- (void)connectionReceived:(NSNotification *)aNotification 
{
//    NSFileHandle * incomingConnection = [[aNotification userInfo] objectForKey:NSFileHandleNotificationFileHandleItem];
//    NSData * representationToSend = [[imageView image] TIFFRepresentation];
//    [[aNotification object] acceptConnectionInBackgroundAndNotify];
//    [incomingConnection writeData:representationToSend];
//    [incomingConnection closeFile];
//    numberOfDownloads++;
}









@end
