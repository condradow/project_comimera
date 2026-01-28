plugins {
    java
    application
    id("com.github.johnrengelman.shadow") version "6.1.0"
}

sourceSets.main.get().java.srcDir("src/main")
sourceSets.main.get().resources.srcDir("src/resources")

application {
    mainClassName = "sc.player.util.Starter"
}

repositories {
    jcenter()
    maven("https://maven.wso2.org/nexus/content/groups/wso2-public/")
    maven("https://jitpack.io")
}

dependencies {
    if(gradle.startParameter.isOffline) {
        implementation(fileTree("lib"))
    } else {
        implementation("com.github.software-challenge.backend", "piranhas_2026", "26.0.7")
        implementation("ch.qos.logback", "logback-classic", "1.3.15")
    }
}

tasks.shadowJar {
    archiveBaseName.set("piranhas_2026_client")
    archiveClassifier.set("")
    destinationDirectory.set(rootDir)
}
