/***************************************************************************
 *  mdns_helper.c
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

#include "mdns_helper.h"
#include <stddef.h>
#include <string.h>
#include <sys/select.h>
#include <sys/types.h>
#include <sys/time.h>
#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <errno.h>


void *_mdns_event_thread(void *arg);
static int _block_on_answer(DNSServiceRef sdRef);
static void DNSSD_API _service_browser_callback(	
								DNSServiceRef		sdRef, 
								DNSServiceFlags		flags,
								uint32_t			interfaceIndex, 
								DNSServiceErrorType	errorCode,
								const char *		name,
								const char *		type,
								const char *		domain, 
								void *				context );
static void DNSSD_API _service_resolve_callback(
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



DNSServiceErrorType mdns_browser_init(struct mdns_data_struct **data, DNSServiceBrowseReply callBack)
{
	DNSServiceErrorType err = kDNSServiceErr_NoError;	

	*data = (struct mdns_data_struct *) malloc(sizeof(struct mdns_data_struct));

	if(*data == NULL)
		return -1;

	memset(*data, 0, sizeof(struct mdns_data_struct));

	printf("Starting the DNSServiceBrowse stuff\n");
	// Start Browser Service
	err = DNSServiceBrowse( &((*data)->serviceRef), 0, 
				kDNSServiceInterfaceIndexAny, "_giver._tcp", NULL, 
				callBack, NULL);
	
	if(err != kDNSServiceErr_NoError)
	{
		free(*data);
		*data = NULL;
		return err;
	}

	if(pthread_create(	&((*data)->serviceThread),
					NULL,
					_mdns_event_thread,
					(*data)->serviceRef) != 0)
		return -1;
	
	return err;
}




void mdns_browser_free(struct mdns_data_struct **data)
{
	if((*data) != NULL)
	{
		// not sure if this needs to be called
		// pthread_cancel(browserStruct->serviceThread);
		DNSServiceRefDeallocate( (*data)->serviceRef );
		free(*data);
		*data = NULL;
	}
}




DNSServiceErrorType mdns_browser_resolve(	uint32_t interface, const char *name, 
							const char *type, const char *domain,
							DNSServiceResolveReply callBack)
{
	DNSServiceRef sdRef;
	DNSServiceErrorType err = kDNSServiceErr_NoError;	
	int rc;

	err = DNSServiceResolve(	&sdRef,
								0,
								interface,
								name,
								type,
								domain,
								callBack,
								NULL);
	if(err != kDNSServiceErr_NoError)
		return err;

	rc = _block_on_answer(sdRef);
	if (rc == DNSServiceRefSockFD(sdRef))
	{
		err = DNSServiceProcessResult(sdRef);
		if (err != kDNSServiceErr_NoError)
		{
			fprintf(stderr, "DNSServiceProcessResult returned %d\n", err);
		}
	}

	DNSServiceRefDeallocate(sdRef);		
	return err;
}




/*
static void register_callback(DNSServiceRef sdRef,
    		    DNSServiceFlags flags,
		    DNSServiceErrorType errorCode,
		    const char *name,
		    const char *regtype,
		    const char *domain,
		    void *context)
{
	if (!context)
		return;

	pyCb_t *cb = (pyCb_t *)context;

	if (errorCode == kDNSServiceErr_NoError) {
		if (cb->reg_callback)
			(*(registerCbDef)cb->reg_callback)(cb->py_reg_callback, 
						errorCode, (char*)name, 
					        (char*)regtype, (char*)domain);
	} else {
		printf("register error: %d\n", errorCode);
	}
	cb->done = 1;
}


int register_new_type(int flags, 
		      int interface, 
		      char * name,
		      char * type,
		      char * domain, 
		      char * host, 
		      short port,
		      short txtLen, 
		      char *txtRecord,
	  	      registerCbDef reg_callback,
		      PyObject * py_reg_callback)
 
{
	DNSServiceRef sdRef;
	DNSServiceErrorType err;
	struct timeval tv;

	pyCb_t *pyCb = (pyCb_t *)malloc(sizeof(pyCb_t));
	pyCb->reg_callback = (void*)reg_callback;
	pyCb->py_reg_callback = py_reg_callback;
	pyCb->done = 0;

	err = DNSServiceRegister(&sdRef, flags, interface, 
			   name, type, domain, 
			   host, port, 
			   txtLen, txtRecord, 
			   (DNSServiceRegisterReply)register_callback, 
			   (void*)pyCb);

	if (err != kDNSServiceErr_NoError) 
		return err;

	tv.tv_sec = 0;
	tv.tv_usec = 5000;

	while (!pyCb->done) {
		Py_BEGIN_ALLOW_THREADS
		err = wait_for_answer(sdRef, &tv);
		Py_END_ALLOW_THREADS
		if (err < 0)
			break;
		else if (err == DNSServiceRefSockFD(sdRef)) 
			err = DNSServiceProcessResult(sdRef);
		tv.tv_sec = 1;
		tv.tv_usec = 0;
		err = 0;
	}

	return err;
}

*/







void *_mdns_event_thread(void *arg)
{
    int stopNow = 0;
	int rc;
	DNSServiceErrorType err;
	DNSServiceRef sdRef;


	printf("New Event Thread Started!\n");

	sdRef = (DNSServiceRef) arg;
		
	if(sdRef == NULL)
		return NULL;
		
    while (!stopNow)
    {
		rc = _block_on_answer(sdRef);
		if (rc < 0)
		{
            stopNow = 1;
		}
		else if (rc == DNSServiceRefSockFD(sdRef))
		{
			err = DNSServiceProcessResult(sdRef);
            if (err != kDNSServiceErr_NoError)
            {
                fprintf(stderr,
                    "DNSServiceProcessResult returned %d\n", err);
                stopNow = 1;
            }
        }
    }

	printf("Event Thread exiting!\n");
	
	return NULL;
}


static int _block_on_answer(DNSServiceRef sdRef)
{
	int result, rc = 0;
	int fd;
    int nfds;
    struct timeval tv;	
    fd_set readfds;
    fd_set* nullFd = (fd_set*) NULL;
	
	
	if (!sdRef)
		return -1;

    fd  = DNSServiceRefSockFD(sdRef);
	nfds = fd + 1;

	// 1. Set up the fd_set as usual here.
	FD_ZERO(&readfds);

	// 2. Add the fd to the fd_set
	FD_SET(fd , &readfds);

	// 3. Set up the timeout to 5 seconds.
	tv.tv_sec = 5;
	tv.tv_usec = 0;

	// wait for pending data or 5 secs to elapse:
	result = select(nfds, &readfds, nullFd, nullFd, &tv);
	if (result > 0)
	{
		if (FD_ISSET(fd , &readfds))
		{
			rc = fd;
			FD_CLR(fd, &readfds);
		}
	}
	else if (result == 0)
	{
		rc = 0;
	}
	else
	{
		rc = result;
	}

	return rc;
}
