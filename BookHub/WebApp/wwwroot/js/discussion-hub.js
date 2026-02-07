// SignalR connection manager for discussions and topics
class DiscussionHubConnection {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.currentGroups = new Set(); // Track joined groups for reconnection
    }

    // Initialize connection
    async initialize() {
        if (this.connection) {
            console.warn('SignalR connection already initialized');
            return this.connection;
        }

        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/discussion")
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        // Exponential backoff: 0, 2, 10, 30 seconds, then 30s intervals
                        if (retryContext.previousRetryCount === 0) return 0;
                        if (retryContext.previousRetryCount === 1) return 2000;
                        if (retryContext.previousRetryCount === 2) return 10000;
                        return 30000;
                    }
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Connection lifecycle handlers
            this.connection.onreconnecting(error => {
                console.warn('SignalR connection lost. Reconnecting...', error);
                this.isConnected = false;
                this.showConnectionStatus('Reconnecting...', 'warning');
            });

            this.connection.onreconnected(connectionId => {
                console.log('SignalR reconnected', connectionId);
                this.isConnected = true;
                this.showConnectionStatus('Connected', 'success');
                // Rejoin all groups after reconnection
                this.rejoinCurrentGroups();
            });

            this.connection.onclose(error => {
                console.error('SignalR connection closed', error);
                this.isConnected = false;
                this.showConnectionStatus('Disconnected', 'error');
            });

            await this.connection.start();
            this.isConnected = true;
            console.log('SignalR connected successfully');
            return this.connection;

        } catch (error) {
            console.error('Failed to initialize SignalR connection:', error);
            this.showConnectionStatus('Connection failed', 'error');
            throw error;
        }
    }

    // Join a discussion group
    async joinDiscussion(discussionId) {
        if (!this.isConnected) {
            await this.initialize();
        }
        await this.connection.invoke("JoinDiscussion", discussionId);
        this.currentGroups.add(`discussion_${discussionId}`);
        console.log(`Joined discussion: ${discussionId}`);
    }

    // Leave a discussion group
    async leaveDiscussion(discussionId) {
        if (this.isConnected && this.connection) {
            await this.connection.invoke("LeaveDiscussion", discussionId);
            this.currentGroups.delete(`discussion_${discussionId}`);
            console.log(`Left discussion: ${discussionId}`);
        }
    }

    // Join a topic group
    async joinTopic(topicId) {
        if (!this.isConnected) {
            await this.initialize();
        }
        await this.connection.invoke("JoinTopic", topicId);
        this.currentGroups.add(`topic_${topicId}`);
        console.log(`Joined topic: ${topicId}`);
    }

    // Leave a topic group
    async leaveTopic(topicId) {
        if (this.isConnected && this.connection) {
            await this.connection.invoke("LeaveTopic", topicId);
            this.currentGroups.delete(`topic_${topicId}`);
            console.log(`Left topic: ${topicId}`);
        }
    }

    // Register handler for receiving messages
    onReceiveMessage(callback) {
        if (this.connection) {
            this.connection.on("ReceiveMessage", callback);
        }
    }

    // Register handler for receiving new topics
    onReceiveTopic(callback) {
        if (this.connection) {
            this.connection.on("ReceiveTopic", callback);
        }
    }

    // Show connection status to user (optional UI feedback)
    showConnectionStatus(message, type) {
        // Simple console logging - can be enhanced with UI notifications
        console.log(`Connection status: ${message} (${type})`);

        // Optional: Display a toast/notification to user
        // Uncomment and customize based on your UI framework
        /*
        if (type === 'error') {
            console.error(message);
        } else if (type === 'warning') {
            console.warn(message);
        } else {
            console.info(message);
        }
        */
    }

    // Rejoin groups after reconnection
    async rejoinCurrentGroups() {
        if (!this.currentGroups.size) return;

        console.log('Rejoining groups after reconnection...');
        for (const group of this.currentGroups) {
            try {
                if (group.startsWith('discussion_')) {
                    const discussionId = group.substring('discussion_'.length);
                    await this.connection.invoke("JoinDiscussion", discussionId);
                } else if (group.startsWith('topic_')) {
                    const topicId = group.substring('topic_'.length);
                    await this.connection.invoke("JoinTopic", topicId);
                }
                console.log(`Rejoined group: ${group}`);
            } catch (error) {
                console.error(`Failed to rejoin group ${group}:`, error);
            }
        }
    }

    // Stop connection
    async stop() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
            this.currentGroups.clear();
            console.log('SignalR connection stopped');
        }
    }
}

// Create global instance
window.discussionHub = new DiscussionHubConnection();
